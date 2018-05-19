using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class PlaylistItem {
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
}
