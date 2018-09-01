using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace CastHelper {
	public static class SidearmSports {
		[DataContract]
		private class Event {
			[DataMember]
			public Formats formats = new Formats();
		}

		[DataContract]
		private class Formats {
			[DataMember]
			public string MobileH264 = "";
		}

		public static async Task<IEnumerable<string>> GetMediaUrlsAsync(Uri uri, int id) {
			var req = WebRequest.CreateHttp(new Uri(uri, $"/services/allaccess.ashx/media/get?id={id}&type=Live"));
			req.Method = "GET";
			req.Accept = "application/json";
			req.UserAgent = Program.UserAgent;
			using (var resp = await req.GetResponseAsync())
			using (var stream = resp.GetResponseStream()) {
				var obj = new DataContractJsonSerializer(typeof(Event[])).ReadObject(stream) as Event[];

				return obj
					.Select(a => a.formats.MobileH264)
					.Where(s => !string.IsNullOrEmpty(s));
			}
		}
	}
}
