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
			var client = new RokuDeviceDiscoveryClient();
			using (var tokenSource = new CancellationTokenSource()) {
				var task = client.DiscoverDevicesAsync(ctx => {
					comboBox1.Items.Add(ctx.Device);
					return Task.FromResult(false);
				}, tokenSource.Token);
				tokenSource.CancelAfter(3000);
				//progressBar1.Maximum = 3000;
				//while (progressBar1.Value < progressBar1.Maximum) {
				//	progressBar1.Value = Math.Min(progressBar1.Value + 250, progressBar1.Maximum);
				//	await Task.Delay(250);
				//}
				try {
					await task;
				} catch (TaskCanceledException) { }
			}
			panel1.Visible = false;

			if (comboBox1.Items.Count == 0) {
				MessageBox.Show(this, "Could not find any Roku devices on the local network.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnPlay_Click(object sender, EventArgs e) {
			btnPlay.Enabled = false;

			try {
				// Get type of media
				string contentType;

				var req = WebRequest.CreateHttp(txtUrl.Text);
				req.Method = "HEAD";
				req.UserAgent = "casthelper.exe/1.0 (https://github.com/IsaacSchemm/casthelper.exe)";
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) {
					contentType = resp.ContentType;
				}

				MessageBox.Show(contentType);
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				MessageBox.Show(this, "The requested URL could not be found. Make sure you haven't mistyped the URL. (HTTP 404)", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
	}
}
