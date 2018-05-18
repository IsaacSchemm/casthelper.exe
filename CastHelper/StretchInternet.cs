using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public static class StretchInternet {
		public static async Task<string> GetMediaUrlAsync(int eventId) {
			var req = WebRequest.CreateHttp($"https://api.stretchinternet.com/trinity/event/tcg/{eventId}");
			req.Method = "GET";
			req.Accept = "application/json";
			req.UserAgent = Program.UserAgent;
			using (var resp = await req.GetResponseAsync())
			using (var sr = new StreamReader(resp.GetResponseStream())) {
				string json = await sr.ReadToEndAsync();
				var obj = JsonConvert.DeserializeAnonymousType(json, new[] {
					new {
						media = new[] {
							new {
								url = ""
							}
						}
					}
				});

				var urls = obj
					.SelectMany(a => a.media.Select(b => b.url))
					.Where(s => !string.IsNullOrEmpty(s))
					.Select(s => $"https://{s}");
				if (urls.Count() < 2) return urls.FirstOrDefault();

				using (var f = new SelectTypeForm<string>("Multiple possible video URLs were found.", urls)) {
					return f.ShowDialog() == DialogResult.OK
						? f.SelectedItem
						: null;
				}
			}
		}
	}
}
