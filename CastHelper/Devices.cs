using System.Threading.Tasks;

namespace CastHelper {
	public enum MediaType {
		Unknown,
		Video,
		Audio
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
