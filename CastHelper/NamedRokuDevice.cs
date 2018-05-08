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
	public class NamedRokuDevice : IRokuDevice, INamedDevice {
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

		public async Task PlayMediaAsync(string mediaUrl, MediaType type, string contentType) {
			try {
				string url = null;
				switch (type) {
					case MediaType.Audio:
						string subtype = contentType.Split('/', ';', ',')[1];
						if (subtype == "mpeg") subtype = "mp3";
						url = $"/input/15985?t=a&u={WebUtility.UrlEncode(mediaUrl)}&songname=(null)&artistname=(null)&songformat={subtype}&albumarturl=(null)";
						break;
					case MediaType.Video:
						url = $"/input/15985?t=v&u={WebUtility.UrlEncode(mediaUrl)}&k=(null)";
						break;
					case MediaType.Image:
						throw new NotImplementedException("Showing images on Roku is not currently supported.");
				}

				var req = WebRequest.CreateHttp(new Uri(Location, url));
				req.Method = "POST";
				req.UserAgent = "CastHelper/1.0 (https://github.com/IsaacSchemm/casthelper.exe)";
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) { }
			} catch (Exception ex) {
				throw new Exception("Could not send media to Roku.", ex);
			}
		}

		public override string ToString() {
			return Name;
		}
	}
}
