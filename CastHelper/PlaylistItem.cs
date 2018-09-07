using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class PlaylistItem : IEquatable<PlaylistItem> {
		public readonly string Name;
		public readonly string Url;

		public PlaylistItem(string name, string url) {
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Url = url ?? throw new ArgumentNullException(nameof(url));
		}

		public override bool Equals(object obj) {
			return this.Equals(obj as PlaylistItem);
		}

		public bool Equals(PlaylistItem other) {
			return other != null &&
				   this.Name == other.Name &&
				   this.Url == other.Url;
		}

		public override int GetHashCode() {
			var hashCode = -1254404684;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Url);
			return hashCode;
		}

		public override string ToString() {
			return Name ?? Url;
		}
	}
}
