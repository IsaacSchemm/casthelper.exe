using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CastHelper {
	public class NamedAppleTV : IVideoDevice {
		public string Name { get; private set; }
		public readonly Uri Location;

		public NamedAppleTV(string name, Uri location) {
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}

		public NamedAppleTV(string name, IPAddress ipAddress) : this(name, new Uri($"http://{ipAddress}:7000")) { }

		public async Task PlayVideoAsync(string url) {
			var req1 = WebRequest.CreateHttp(new Uri(Location, "/play"));
			req1.Method = "POST";
			req1.UserAgent = Program.UserAgent;
			req1.ContentType = "text/parameters";
			using (var sw = new StreamWriter(await req1.GetRequestStreamAsync())) {
				await sw.WriteLineAsync($"Content-Location: " + url);
				await sw.WriteLineAsync($"Start-Position: 0");
			}
			using (var resp = await req1.GetResponseAsync())
			using (var s = resp.GetResponseStream()) { }

			// Apple TV initiates a reverse HTTP connection, but instead of listening to it, we'll just do this
			using (var f = new AppleTVPlaybackForm())
			using (Timer timer = new Timer(1000)) {
				f.LabelText = $"Currently playing video on {Name}. Closing this window will stop playback.";
				timer.Elapsed += async (o, e) => {
					try {
						var req = WebRequest.CreateHttp(new Uri(Location, "/scrub"));
						req.Method = "GET";
						req.UserAgent = Program.UserAgent;
						using (var resp = await req.GetResponseAsync())
						using (var sr = new StreamReader(resp.GetResponseStream())) {
							string line;
							double duration = 0, position = 0;
							while ((line = await sr.ReadLineAsync()) != null) {
								if (line.StartsWith("duration: ")) {
									duration = double.Parse(line.Substring(10));
								} else if (line.StartsWith("position: ")) {
									position = double.Parse(line.Substring(10));
								}
							}

							f.BeginInvoke(new Action(() => {
								if (position == 0 && f.PositionSec != 0) {
									f.OkToClose = true;
									f.Close();
								} else {
									f.DurationSec = Math.Max((int)duration, f.PositionSec);
									f.PositionSec = Math.Min((int)position, f.DurationSec);
								}
							}));
						}
					} catch (Exception) { }
				};
				timer.Start();
				f.ShowDialog();
				timer.Stop();
			}

			var req2 = WebRequest.CreateHttp(new Uri(Location, "/stop"));
			req2.Method = "POST";
			req2.UserAgent = Program.UserAgent;
			using (var resp = await req2.GetResponseAsync())
			using (var s = resp.GetResponseStream()) { }
		}

		public override string ToString() {
			return $"{Name} ({Location.Host})";
		}
	}
}
