using RokuDotNet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
		}

		private async void Form1_Shown(object sender, EventArgs e) {
			btnPlay.Enabled = false;

			var client = new RokuDeviceDiscoveryClient();
			using (var tokenSource = new CancellationTokenSource()) {
				var task = client.DiscoverDevicesAsync(ctx => {
					comboBox1.Items.Add(ctx.Device);
					return Task.FromResult(false);
				}, tokenSource.Token);
				tokenSource.CancelAfter(3000);
				try {
					await task;
				} catch (TaskCanceledException) { }
			}
			panel1.Visible = false;

			if (comboBox1.Items.Count == 0) {
				MessageBox.Show(this, "Could not find any Roku devices on the local network.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			btnPlay.Enabled = true;
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			btnPlay.Enabled = false;

			try {
				// Get type of media
				string contentType;

				var req = WebRequest.CreateHttp(txtUrl.Text);
				req.Method = "HEAD";
				req.Accept = "application/vnd.apple.mpegurl,application/dash+xml,application/vnd.ms-sstr+xml,video/*,audio/*,image/*,*/*;q=0.9";
				req.UserAgent = "casthelper.exe/1.0 (https://github.com/IsaacSchemm/casthelper.exe)";
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) {
					contentType = resp.ContentType;
				}

				string type = contentType.Split('/').First();
				switch (type) {
					case "audio":
					case "video":
					case "image":
						break;
					case "text":
						MessageBox.Show(this, "This URL refers to a web page or document, not to a video, audio, or photo resource.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						break;
					default:
						if (contentType.StartsWith("application/vnd.apple.mpegurl")) {
							type = "video";
						} else if (contentType.StartsWith("application/dash+xml")) {
							type = "video";
						} else if (contentType.StartsWith("application/vnd.ms-sstr+xml")) {
							type = "video";
						} else {
							using (var f = new SelectTypeForm()) {
								if (f.ShowDialog(this) == DialogResult.OK) {
									type = f.SelectedType;
								}
							}
						}
						break;
				}

				string url = null;
				switch (type) {
					case "audio":
						url = $"/input/15985?t=a&u={WebUtility.UrlEncode(txtUrl.Text)}&k=(null)";
						break;
					case "video":
						url = $"/input/15985?t=v&u={WebUtility.UrlEncode(txtUrl.Text)}&k=(null)";
						break;
					case "image":
						url = $"/input/15985?t=p&u={WebUtility.UrlEncode(txtUrl.Text)}";
						break;
				}

				if (url != null) {
					MessageBox.Show(url);
				}
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				MessageBox.Show(this, "No media was found at the given URL - make sure that you have typed the URL correctly. For live streams, this may also mean that the stream has not yet started. (HTTP 404)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode != null) {
				int status = (int)(ex.Response as HttpWebResponse)?.StatusCode;
				MessageBox.Show(this, $"An unknown error occurred. (HTTP {status})", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (FormatException ex) {
				MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			} catch (Exception ex) {
				Console.Error.WriteLine(ex);
				MessageBox.Show(this, "An unknown error occurred.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			btnPlay.Enabled = true;
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			Close();
		}
	}
}
