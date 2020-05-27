using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CastHelper {
	/// <summary>
	/// Allows CastHelper to parse an M3U playlist given to it by a website to present the user with a set of options.
	/// </summary>
	public static class Disambiguation {
		public static async Task<IReadOnlyList<PlaylistItem>> ParseM3uAsync(string contents) {
			using (var sr = new StringReader(contents)) {
				return await ParseM3uAsync(sr);
			}
		}

		public static async Task<IReadOnlyList<PlaylistItem>> ParseM3uAsync(TextReader sr) {
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

			return list;
		}
	}
}
