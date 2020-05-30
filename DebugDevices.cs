using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public class VLCDevice : IVideoDevice, IAudioDevice {
		public string Name => "VLC";

		public Task PlayAudioAsync(string url, string contentType) {
			return PlayVideoAsync(url);
		}

		public Task PlayVideoAsync(string url) {
			string[] paths = new[] {
				Environment.ExpandEnvironmentVariables("%ProgramW6432%"),
				Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%")
			};
			foreach (string programFilesDir in paths) {
				string vlcPath = Path.Combine(programFilesDir, "VideoLan", "VLC", "vlc.exe");
				if (File.Exists(vlcPath)) {
					Process.Start(vlcPath, url);
					return Task.CompletedTask;
				}
			}
			MessageBox.Show("Could not find VLC in: " + string.Join("; ", paths), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return Task.CompletedTask;
		}

		public override string ToString() => Name;
	}
}
