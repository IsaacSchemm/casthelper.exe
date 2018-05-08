using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class NamedAppleTV : IVideoDevice {
		public string Name { get; private set; }
		public readonly Uri Location;

		public NamedAppleTV(string name, Uri location) {
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}

		public NamedAppleTV(string name, IPAddress ipAddress) : this(name, new Uri($"http://{ipAddress}:7000")) { }

		public override string ToString() {
			return Name;
		}

		public async Task PlayVideoAsync(string url) {
			var req = WebRequest.CreateHttp(new Uri(Location, "/play"));
			req.Method = "POST";
			req.UserAgent = Program.UserAgent;
			req.ContentType = "text/parameters";
			using (var sw = new StreamWriter(await req.GetRequestStreamAsync())) {
				await sw.WriteLineAsync($"Content-Location: " + url);
				await sw.WriteLineAsync($"Start-Position: 0");
			}
			using (var resp = await req.GetResponseAsync())
			using (var s = resp.GetResponseStream()) { }
		}
	}
}
