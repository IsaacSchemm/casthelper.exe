using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public static class Disambiguation {
		private class PlaylistItem {
			public readonly string Name;
			public readonly string Url;

			public PlaylistItem(string name, string url) {
				Name = name ?? throw new ArgumentNullException(nameof(name));
				Url = url ?? throw new ArgumentNullException(nameof(url));
			}

			public override string ToString() {
				return Name ?? Url;
			}
		}

		public static async Task<string> DisambiguateAsync(Uri uri, CookieContainer cookieContainer = null) {
			// Retrieve and display the HTML content.
			var req = WebRequest.CreateHttp(uri);
			req.Method = "GET";
			req.Accept = Program.Accept;
			req.UserAgent = Program.UserAgent;
			req.AllowAutoRedirect = false;
			if (cookieContainer != null) {
				req.CookieContainer = cookieContainer;
			}
			using (var resp = await req.GetResponseAsync())
			using (var sr = new StreamReader(resp.GetResponseStream())) {
				if (resp.ContentType.StartsWith("text/html") || resp.ContentType.StartsWith("application/xml+xhtml")) {
					return VideoUrlFinder.GetVideoUriFromHtml(await sr.ReadToEndAsync());
				} else if (resp.ContentType.StartsWith("audio/mpegurl")) {
					if (await sr.ReadLineAsync() != "#EXTM3U") {
						// Playlist header invalid
						return null;
					}

					List<PlaylistItem> list = new List<PlaylistItem>();
					string name = null;
					string line;
					while ((line = await sr.ReadLineAsync()) != null) {
						if (string.IsNullOrWhiteSpace(line)) continue;
						if (line.StartsWith("#EXTINF:")) {
							line = line.Substring("#EXTINF:".Length);
							int comma = line.IndexOf(',');
							name = line.Substring(comma + 1);
						} else if (!line.StartsWith("#")) {
							list.Add(new PlaylistItem(name, line));
							name = null;
						}
					}

					using (var f = new SelectTypeForm<PlaylistItem>("Multiple possible video URLs were found.", list)) {
						return f.ShowDialog() == DialogResult.OK
							? f.SelectedItem?.Url
							: null;
					}
				} else {
					return null;
				}
			}
		}
	}
}
