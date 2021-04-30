﻿using CrossInterfaceRokuDeviceDiscovery;
using RokuDotNet.Client;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zeroconf;

namespace CastHelper {
	public partial class MainForm : Form {
		private readonly CookieContainer _cookieContainer;
		private readonly IRokuDeviceDiscoveryClient _discoveryClient;

		public string Url {
			get {
				return txtUrl.Text;
			}
			set {
				txtUrl.Text = value;
			}
		}

		public MainForm() {
			InitializeComponent();
			_cookieContainer = new CookieContainer();

			var addresses = Dns.GetHostEntry(Dns.GetHostName())
				.AddressList
				.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
				.ToList();
			_discoveryClient = new CrossInterfaceRokuDeviceDiscoveryClient(addresses);
		}

		private async void AddRoku(IHttpRokuDevice device) {
			try {
				var deviceInfo = await device.GetDeviceInfoAsync();
				string name = deviceInfo.UserDeviceName;
				if (string.IsNullOrEmpty(name)) name = deviceInfo.ModelName;
				AddDevice(new NamedRokuDevice(device, name));
			} catch (Exception) {
				AddDevice(new NamedRokuDevice(device, device.Id));
			}
		}

		private void AddDevice(IDevice device) {
			comboBox1.Items.Add(device);
			if (comboBox1.SelectedIndex == -1) {
				comboBox1.SelectedIndex = 0;
				txtUrl.Focus();
				panel1.Visible = false;
				btnPlay.Enabled = true;
			}
		}

		private void Form1_Shown(object sender, EventArgs e) {
			Rescan();
		}

		private async Task<string> FollowRedirectsToContentTypeAsync(string method = "HEAD") {
			try {
				var req = WebRequest.CreateHttp(txtUrl.Text);
				req.Method = method;
				req.Accept = Program.Accept;
				req.UserAgent = Program.UserAgent;
				req.CookieContainer = _cookieContainer;
				using (var resp = await req.GetResponseAsync()) {
					txtUrl.Text = resp.ResponseUri.AbsoluteUri;
					return resp.ContentType;
				}
			} catch (WebException ex) when (method == "HEAD" && (ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.MethodNotAllowed) {
				return await FollowRedirectsToContentTypeAsync("GET");
			} catch (WebException ex) when (method == "HEAD" && (ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				return await FollowRedirectsToContentTypeAsync("GET");
			}
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			comboBox1.Enabled = false;
			txtUrl.Enabled = false;
			btnPlay.Enabled = false;

			foreach (var o in comboBox1.Items) {
				if (o is NamedRokuDevice d) {
					d.ShowControls = showPlaybackControlWindowToolStripMenuItem.Checked;
				}
			}
			
			try {
				// Get type of media
				string contentType = await FollowRedirectsToContentTypeAsync();
				
				if (contentType != null) {
					MediaType type;
					switch (contentType.Split('/').First().ToLowerInvariant()) {
						case "audio":
							type = MediaType.Audio;
							break;
						case "video":
							type = MediaType.Video;
							break;
						case "application":
							type = contentType.IndexOf("mpegurl", StringComparison.InvariantCultureIgnoreCase) >= 0
								? MediaType.Video
								: MediaType.Unknown;
							break;
						default:
							type = MediaType.Unknown;
							break;
					}

					switch (type) {
						case MediaType.Video:
							if (!(comboBox1.SelectedItem is IVideoDevice videoDevice)) {
								throw new NotImplementedException("CastHelper cannot cast video to this device.");
							}
							Hide();
							await videoDevice.PlayVideoAsync(txtUrl.Text);
							Close();
							return;
						case MediaType.Audio:
							if (!(comboBox1.SelectedItem is IAudioDevice audioDevice)) {
								throw new NotImplementedException("CastHelper cannot cast audio to this device.");
							}
							Hide();
							await audioDevice.PlayAudioAsync(txtUrl.Text, contentType);
							Close();
							return;
						default:
							MessageBox.Show(this, $"CastHelper does not recognize the content type {contentType}.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			Show();
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
			MessageBox.Show(this, @"CastHelper 3.0
Copyright © 2018-2021 Isaac Schemm
https://github.com/IsaacSchemm/casthelper.exe

See casthelper-licenses.txt for license information.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				components?.Dispose();
			}
			base.Dispose(disposing);
		}

		private async void RokuRescan() {
			try {
				await _discoveryClient.DiscoverDevicesAsync(context => {
					BeginInvoke(new Action(() => {
						if (context.Device is IHttpRokuDevice r)
							AddRoku(r);
					}));
					return Task.FromResult(false);
				});
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				MessageBox.Show(this, "Could not scan for Roku devices.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void AppleTVRescan() {
			try {
				var list = await ZeroconfResolver.ResolveAsync("_airplay._tcp.local.", TimeSpan.FromSeconds(5));
				foreach (var service in list) {
					var addresses = service.IPAddresses
						.Select(a => IPAddress.Parse(a))
						.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
					foreach (var address in addresses)
						BeginInvoke(new Action(() => AddDevice(new NamedAppleTV(service.DisplayName, address))));
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				MessageBox.Show(this, "Could not scan for Apple TV devices.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void Rescan() {
			comboBox1.Items.Clear();

			RokuRescan();
			AppleTVRescan();

			//AddDevice(new VLCDevice());
		}

		private void rescanToolStripMenuItem1_Click(object sender, EventArgs e) {
			btnPlay.Enabled = false;
			panel1.Visible = true;
			Rescan();
		}

		private void manuallyEnterIPAddressToolStripMenuItem_Click_1(object sender, EventArgs e) {
			string ip = Microsoft.VisualBasic.Interaction.InputBox("Enter the IP address of the Roku on your local network (as shown in Settings > Network > About):", "Add Roku");
			if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out IPAddress _)) {
				var device = new HttpRokuDevice($"roku.custom.{ip}", new Uri($"http://{ip}:8060/"));
				AddRoku(device);
			}
		}
	}
}
