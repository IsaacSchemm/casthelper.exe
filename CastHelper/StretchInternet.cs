using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public static class StretchInternet {
		[DataContract]
		private class Event {
			[DataMember]
			public Media[] media;
		}

		[DataContract]
		private class Media {
			[DataMember]
			public string url;
		}

		public static async Task<IEnumerable<string>> GetMediaUrlsAsync(int eventId) {
			var req = WebRequest.CreateHttp($"https://api.stretchinternet.com/trinity/event/tcg/{eventId}");
			req.Method = "GET";
			req.Accept = "application/json";
			req.UserAgent = Program.UserAgent;
			using (var resp = await req.GetResponseAsync())
			using (var stream = resp.GetResponseStream()) {
				var obj = new DataContractJsonSerializer(typeof(Event[])).ReadObject(stream) as Event[];

				return obj
					.SelectMany(a => a.media.Select(b => b.url))
					.Where(s => !string.IsNullOrEmpty(s))
					.Select(s => $"https://{s}");
			}
		}

		public static async Task<string> GetMediaUrlAsync(int eventId) {
			var urls = await GetMediaUrlsAsync(eventId);
			if (urls.Count() < 2) return urls.FirstOrDefault();

			using (var f = new SelectTypeForm<string>("Multiple possible video URLs were found.", urls)) {
				return f.ShowDialog() == DialogResult.OK
					? f.SelectedItem
					: null;
			}
		}
	}
}
