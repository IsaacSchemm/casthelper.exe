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

		public Task PlayVideoAsync(string url) {
			Process.Start("microsoft-edge:" + url);
			return Task.CompletedTask;
		}

		public override string ToString() {
			return "Microsoft Edge (local PC)";
		}
	}
}
