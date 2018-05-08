using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public enum MediaType {
		Unknown,
		Video,
		Audio,
		Image,
		Text
	}

	public interface IVideoDevice {
		string Name { get; }
		Task PlayVideoAsync(string url);
	}

	public interface IAudioDevice {
		string Name { get; }
		Task PlayAudioAsync(string url, string contentType);
	}
}
