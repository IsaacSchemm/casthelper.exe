using RokuDotNet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zeroconf;

namespace CastHelper {
	public partial class Form1 : Form {
		private readonly CookieContainer _cookieContainer;
		private readonly RokuDeviceDiscoveryClient _discoveryClient;
		private IObservable<IZeroconfHost> _appleTvResolver;
		private IDisposable _appleTvListener;

		public string Url {
			get {
				return txtUrl.Text;
			}
			set {
				txtUrl.Text = value;
			}
		}

		public Form1() {
			InitializeComponent();
			_cookieContainer = new CookieContainer();

			_discoveryClient = new RokuDeviceDiscoveryClient();
			_discoveryClient.DeviceDiscovered += (o, e) => BeginInvoke(new Action(() => AddRoku(e.Device)));
		}

		private async void AddRoku(IRokuDevice device) {
			try {
				var deviceInfo = await device.Query.GetDeviceInfoAsync();
				string name = deviceInfo.UserDeviceName;
				if (string.IsNullOrEmpty(name)) name = deviceInfo.ModelName;
				AddDevice(new NamedRokuDevice(device, name));
			} catch (Exception) {
				AddDevice(new NamedRokuDevice(device, device.Id));
			}
		}

		private void AddDevice(object device) {
			comboBox1.Items.Add(device);
			if (comboBox1.SelectedIndex == -1) {
				comboBox1.SelectedIndex = 0;
				txtUrl.Focus();
				panel1.Visible = false;
				btnPlay.Enabled = true;
			}
		}

		private void Form1_Shown(object sender, EventArgs e) {
			btnPlay.Enabled = false;
			_discoveryClient.DiscoverDevicesAsync();

			_appleTvResolver = ZeroconfResolver.ResolveContinuous("_airplay._tcp.local.");
			_appleTvListener = _appleTvResolver.Subscribe(service => {
				var address = service.IPAddresses.Select(a => IPAddress.Parse(a)).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
				if (address != null) {
					BeginInvoke(new Action(() => AddDevice(new NamedAppleTV(service.DisplayName, address))));
				}
			});

#if DEBUG
			Task.Delay(5000).ContinueWith(t => {
				BeginInvoke(new Action(() => AddDevice(new EdgeDevice())));
			});
#endif
		}
		
		private async Task<string> FollowRedirectsToContentTypeAsync() {
			for (int i = 0; i < 50; i++) {
				Uri uri = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out Uri tmp1) ? tmp1
						: Uri.TryCreate("http://" + txtUrl.Text, UriKind.Absolute, out Uri tmp2) ? tmp2
							: throw new FormatException("You must enter a valid URL.");

				if (uri.Host == "portal.stretchinternet.com") {
					var m = Regex.Match(uri.Query, "eventId=([0-9]+)");
					if (m.Success && int.TryParse(m.Groups[1].Value, out int eventId)) {
						try {
							string newUrl = await StretchInternet.GetMediaUrlAsync(eventId);
							if (newUrl != null) {
								txtUrl.Text = newUrl;
								continue;
							}
						} catch (Exception ex) {
							Console.Error.WriteLine(ex);
						}
					}
				}

				var req = WebRequest.CreateHttp(uri);
				req.Method = "HEAD";
				req.Accept = Program.Accept;
				req.UserAgent = Program.UserAgent;
				req.AllowAutoRedirect = false;
				req.CookieContainer = _cookieContainer;
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) {
					int? code = (int?)(resp as HttpWebResponse)?.StatusCode;
					if (new[] {
						"text/html",
						"application/xml+xhtml"
					}.Any(x => resp.ContentType.StartsWith(x))) {
						string newUrl = await VideoUrlFinder.GetVideoUriFromUriAsync(req.RequestUri, _cookieContainer);
						if (newUrl != null) {
							txtUrl.Text = newUrl;
						} else {
							return resp.ContentType;
						}
					} else if (code == 300 && resp.ContentType.StartsWith("audio/mpegurl")) {
						string newUrl = await Disambiguation.DisambiguateM3uAsync(req.RequestUri, _cookieContainer);
						if (newUrl != null) {
							txtUrl.Text = newUrl;
						} else {
							return null;
						}
					} else if (code / 100 == 3) {
						// Redirect
						txtUrl.Text = new Uri(uri, resp.Headers["Location"]).AbsoluteUri;
					} else {
						return resp.ContentType;
					}
				}
			}

			throw new Exception("Too many redirects");
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			comboBox1.Enabled = false;
			txtUrl.Enabled = false;
			btnPlay.Enabled = false;
			
			try {
				// Get type of media
				string contentType = await FollowRedirectsToContentTypeAsync();
				contentType = contentType.ToLowerInvariant();
				
				if (contentType != null) {
					MediaType type = MediaType.Unknown;
					switch (contentType.Split('/').First()) {
						case "audio":
							type = MediaType.Audio;
							break;
						case "video":
							type = MediaType.Video;
							break;
						case "image":
							type = MediaType.Image;
							break;
						case "text":
							type = MediaType.Text;
							break;
						default:
							if (contentType.StartsWith("application/dash+xml")) {
								type = MediaType.Video;
							} else if (contentType.StartsWith("application/vnd.ms-sstr+xml")) {
								type = MediaType.Video;
							}
							break;
					}

					if (contentType.Contains("mpegurl")) {
						// Assume HLS
						type = MediaType.Video;
					}

					if (type == MediaType.Unknown) {
						using (var f = new SelectForm<MediaType>("Could not detect media type. Please select a type below:", (MediaType[])Enum.GetValues(typeof(MediaType)))) {
							if (f.ShowDialog(this) == DialogResult.OK) {
								type = f.SelectedItem;
							}
						}
					}

					switch (type) {
						case MediaType.Video:
							var videoDevice = comboBox1.SelectedItem as IVideoDevice;
							if (videoDevice == null) {
								throw new NotImplementedException("CastHelper cannot cast video to this device.");
							}
							Hide();
							await videoDevice.PlayVideoAsync(txtUrl.Text);
							Close();
							break;
						case MediaType.Audio:
							var audioDevice = comboBox1.SelectedItem as IAudioDevice;
							if (audioDevice == null) {
								throw new NotImplementedException("CastHelper cannot cast audio to this device.");
							}
							Hide();
							await audioDevice.PlayAudioAsync(txtUrl.Text, contentType);
							Close();
							break;
						case MediaType.Image:
							throw new NotImplementedException("CastHelper cannot cast photos to this device.");
						case MediaType.Text:
							MessageBox.Show(this, "This URL refers to a web page or document, not to a raw media file or stream.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
							break;
					}
				}
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				MessageBox.Show(this, "No media was found at the given URL - make sure that you have typed the URL correctly. For live streams, this may also mean that the stream has not yet started. (HTTP 404)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotAcceptable) {
				MessageBox.Show(this, "This URL is not available in a format that Cast Helper supports. (HTTP 406)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.Gone) {
				MessageBox.Show(this, "This URL no longer exists. You might need to obtain a new URL. (HTTP 410)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode != null) {
				int status = (int)(ex.Response as HttpWebResponse)?.StatusCode;
				MessageBox.Show(this, $"An unknown error occurred. (HTTP {status})", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (FormatException ex) {
				MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (NotImplementedException ex) {
				MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				MessageBox.Show(this, "An unknown error occurred.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			comboBox1.Enabled = true;
			txtUrl.Enabled = true;
			btnPlay.Enabled = true;
		}
		
		private void btnCancel_Click(object sender, EventArgs e) {
			Close();
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Close();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show(this, @"CastHelper 1.2
Copyright © 2018 Isaac Schemm
https://github.com/IsaacSchemm/casthelper.exe

RokuDotNet
Copyright © 2018 Phillip Hoff

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Zeroconf and System.Reactive, which are included for Apple TV discovery, are available under the Microsoft Public License and the Apache License 2.0, respectively.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				components?.Dispose();
				_appleTvListener?.Dispose();
			}
			base.Dispose(disposing);
		}

		private void rescanToolStripMenuItem_Click_1(object sender, EventArgs e) {
			_discoveryClient.DiscoverDevicesAsync();
		}

		private void manuallyEnterIPAddressToolStripMenuItem_Click_1(object sender, EventArgs e) {
			using (var f = new InputBox()) {
				f.Text = "Add Roku";
				string ip = f.ShowDialog(this) == DialogResult.OK ? f.InputText : null;
				if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress addr)) {
					var device = new RokuDevice(new Uri($"http://{ip}:8060/"), $"roku.custom.{ip}");
					AddRoku(device);
				}
			}
		}
	}
}
