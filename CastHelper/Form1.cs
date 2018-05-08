using Network.Bonjour;
using RokuDotNet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public partial class Form1 : Form {
		private readonly CookieContainer _cookieContainer;
		private readonly RokuDeviceDiscoveryClient _discoveryClient;
		private readonly BonjourServiceResolver _resolver;

		public Form1() {
			InitializeComponent();
			_cookieContainer = new CookieContainer();

			_discoveryClient = new RokuDeviceDiscoveryClient();
			_discoveryClient.DeviceDiscovered += (o, e) => BeginInvoke(new Action(async () => {
				try {
					var deviceInfo = await e.Device.Query.GetDeviceInfoAsync();
					string name = deviceInfo.UserDeviceName;
					if (string.IsNullOrEmpty(name)) name = deviceInfo.ModelName;
					AddDevice(new NamedRokuDevice(e.Device, name));
				} catch (Exception) {
					AddDevice(new NamedRokuDevice(e.Device, e.Location.ToString()));
				}
			}));

			_resolver = new BonjourServiceResolver();
			_resolver.ServiceFound += service => {
				var address = service.Addresses.SelectMany(a => a.Addresses).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
				if (address != null) {
					BeginInvoke(new Action(() => AddDevice(new NamedAppleTV(service.Name, address))));
				}
			};
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
			_resolver.Resolve("_airplay._tcp.local.");
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			comboBox1.Enabled = false;
			txtUrl.Enabled = false;
			btnPlay.Enabled = false;
			
			try {
				// Get type of media
				string contentType = null;

				for (int i = 0; i < 50; i++) {
					Uri uri = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out Uri tmp1) ? tmp1
						: Uri.TryCreate("http://" + txtUrl.Text, UriKind.Absolute, out Uri tmp2) ? tmp2
							: throw new FormatException("You must enter a valid URL.");

					var req = WebRequest.CreateHttp(uri);
					req.Method = "HEAD";
					req.Accept = "application/vnd.apple.mpegurl,application/dash+xml,application/vnd.ms-sstr+xml,video/*,audio/*";
					req.UserAgent = "CastHelper/1.0 (https://github.com/IsaacSchemm/casthelper.exe)";
					req.AllowAutoRedirect = false;
					req.CookieContainer = _cookieContainer;
					using (var resp = await req.GetResponseAsync())
					using (var s = resp.GetResponseStream()) {
						int? code = (int?)(resp as HttpWebResponse)?.StatusCode;
						if (code / 100 == 3) {
							// redirect
							txtUrl.Text = new Uri(uri, resp.Headers["Location"]).AbsoluteUri;
						} else {
							contentType = resp.ContentType;
							break;
						}
					}
				}

				if (contentType == null) {
					throw new Exception("This URL redirected too many times.");
				}

				MediaType type = MediaType.Unknown;
				switch (contentType.Split('/').First()) {
					case "audio":
						type = MediaType.Audio;
						if (contentType == "audio/x-mpegurl") {
							type = MediaType.Video;
						}
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
						if (contentType.StartsWith("application/vnd.apple.mpegurl")) {
							type = MediaType.Video;
						} else if (contentType.StartsWith("application/dash+xml")) {
							type = MediaType.Video;
						} else if (contentType.StartsWith("application/vnd.ms-sstr+xml")) {
							type = MediaType.Video;
						}
						break;
				}
				
				if (type == MediaType.Unknown) {
					using (var f = new SelectTypeForm()) {
						if (f.ShowDialog(this) == DialogResult.OK) {
							type = f.SelectedType;
						}
					}
				}

				switch (type) {
					case MediaType.Video:
						var videoDevice = comboBox1.SelectedItem as IVideoDevice;
						if (videoDevice == null) {
							throw new NotImplementedException("CastHelper cannot cast video to this device.");
						}
						await videoDevice.PlayVideoAsync(txtUrl.Text);
						break;
					case MediaType.Audio:
						var audioDevice = comboBox1.SelectedItem as IAudioDevice;
						if (audioDevice == null) {
							throw new NotImplementedException("CastHelper cannot cast audio to this device.");
						}
						await audioDevice.PlayAudioAsync(txtUrl.Text, contentType);
						break;
					case MediaType.Image:
						throw new NotImplementedException("CastHelper cannot cast photos to this device.");
					case MediaType.Text:
						MessageBox.Show(this, "This URL refers to a web page or document, not to a video, audio, or photo resource.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						break;
				}
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				MessageBox.Show(this, "No media was found at the given URL - make sure that you have typed the URL correctly. For live streams, this may also mean that the stream has not yet started. (HTTP 404)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		
		private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
			MessageBox.Show(this, @"CastHelper 1.0
Copyright © 2018 Isaac Schemm

RokuDotNet
Copyright © 2018 Phillip Hoff

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
