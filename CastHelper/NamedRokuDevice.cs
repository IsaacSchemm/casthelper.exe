using RokuDotNet.Client;
using RokuDotNet.Client.Input;
using RokuDotNet.Client.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public class NamedRokuDevice : IRokuDevice, IVideoDevice, IAudioDevice {
		private IRokuDevice _device;
		public string Name { get; private set; }

		public NamedRokuDevice(IRokuDevice device, string name) {
			_device = device ?? throw new ArgumentNullException(nameof(device));
			Name = name ?? throw new ArgumentNullException(nameof(name));
		}

		public string Id => _device.Id;
		public IRokuDeviceInput Input => _device.Input;
		public IRokuDeviceQuery Query => _device.Query;
		public Uri Location => (_device as RokuDevice)?.Location;

		public async Task PlayVideoAsync(string mediaUrl) {
			try {
				var req = WebRequest.CreateHttp(new Uri(Location, $"/input/15985?t=v&u={WebUtility.UrlEncode(mediaUrl)}&k=(null)"));
				req.Method = "POST";
				req.UserAgent = Program.UserAgent;
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) { }
			} catch (Exception ex) {
				throw new Exception("Could not send media to Roku.", ex);
			}
		}

		public async Task PlayAudioAsync(string mediaUrl, string contentType = null) {
			try {
				string mediatype;
				switch (contentType) {
					case "audio/mpeg":
						mediatype = "mp3";
						break;
					case "audio/mp4":
						mediatype = "m4a";
						break;
					default:
						mediatype = mediaUrl.Split('.').Last();
						break;
				}
				var req = WebRequest.CreateHttp(new Uri(Location, $"/input/15985?t=a&u={WebUtility.UrlEncode(mediaUrl)}&songname=(null)&artistname=(null)&songformat={WebUtility.UrlEncode(mediatype)}&albumarturl=(null)"));
				req.Method = "POST";
				req.UserAgent = Program.UserAgent;
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) { }
			} catch (Exception ex) {
				throw new Exception("Could not send media to Roku.", ex);
			}
		}

		public override string ToString() {
			return $"{Name} ({Location.Host})";
		}
	}
}
