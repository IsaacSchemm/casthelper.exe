using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public enum MediaType {
		Video,
		Audio,
		Image
	}

	public interface INamedDevice {
		string Name { get; }

		Task PlayMediaAsync(string url, MediaType type, string contentType);
	}
}
