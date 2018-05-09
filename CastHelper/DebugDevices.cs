using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public class EdgeDevice : IVideoDevice, IAudioDevice {
		public string Name => "Microsoft Edge";

		public Task PlayAudioAsync(string url, string contentType) {
			return PlayVideoAsync(url);
		}

		public async Task PlayVideoAsync(string url) {
			Process.Start("microsoft-edge:" + url);

			using (var f = new Form()) {
				f.Controls.Add(new Label {
					Text = "Testing (will close in 3 seconds)",
					Dock = DockStyle.Fill
				});
				f.Show();
				await Task.Delay(3000);
				f.Close();
			}
		}

		public override string ToString() {
			return "Microsoft Edge (local PC)";
		}
	}
}
