using RokuDotNet.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
	}
}
