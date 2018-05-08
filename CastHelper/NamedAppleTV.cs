using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class NamedAppleTV : INamedDevice {
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

		public async Task PlayMediaAsync(string url, MediaType type, string contentType) {
			if (type != MediaType.Video) throw new NotImplementedException("Cannot send audio or images to Apple TV (not implemented)");

			var req = WebRequest.CreateHttp(new Uri(Location, "/play"));
			req.Method = "POST";
			req.UserAgent = "CastHelper/1.0 (https://github.com/IsaacSchemm/casthelper.exe)";
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
