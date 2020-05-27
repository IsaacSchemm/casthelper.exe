using System.Threading.Tasks;

namespace CastHelper {
	public enum MediaType {
		Unknown,
		Video,
		Audio
	}

	public interface IDevice {
		string Name { get; }
	}

	public interface IVideoDevice : IDevice {
		Task PlayVideoAsync(string url);
	}

	public interface IAudioDevice : IDevice {
		Task PlayAudioAsync(string url, string contentType);
	}
}
