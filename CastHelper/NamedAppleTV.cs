using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class NamedAppleTV {
		public readonly string Name;
		public readonly Uri Location;

		public NamedAppleTV(string name, Uri location) {
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Location = location ?? throw new ArgumentNullException(nameof(location));
		}

		public NamedAppleTV(string name, IPAddress ipAddress) : this(name, new Uri($"http://{ipAddress}:7000")) { }

		public override string ToString() {
			return Name;
		}
	}
}
