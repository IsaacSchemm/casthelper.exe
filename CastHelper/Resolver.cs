using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CastHelper {
	public class ResolverResult {
		public readonly string ContentType;
		public readonly IEnumerable<PlaylistItem> Links;

		public ResolverResult(string contentType) {
			ContentType = contentType;
			Links = Enumerable.Empty<PlaylistItem>();
		}

		public ResolverResult(string contentType, IEnumerable<PlaylistItem> links) {
			ContentType = contentType;
			Links = links?.ToList() ?? throw new ArgumentNullException(nameof(links));
		}

		public ResolverResult(string contentType, IEnumerable<string> links) : this(contentType, links.Select(x => new PlaylistItem(x, x))) { }
	}

	public static class Resolver {
		public static async Task<ResolverResult> ResolveAsync(string url, CookieContainer cookieContainer = null) {
			Uri uri = Uri.TryCreate(url, UriKind.Absolute, out Uri tmp1) ? tmp1
				: Uri.TryCreate("http://" + url, UriKind.Absolute, out Uri tmp2) ? tmp2
					: throw new FormatException("You must enter a valid URL.");
			return await ResolveAsync(uri, cookieContainer);
		}

		public static async Task<ResolverResult> ResolveAsync(Uri uri, CookieContainer cookieContainer = null) {
			if (uri.Host == "portal.stretchinternet.com") {
				var m = Regex.Match(uri.Query, "eventId=([0-9]+)");
				if (m.Success && int.TryParse(m.Groups[1].Value, out int eventId)) {
					try {
						var urls = await StretchInternet.GetMediaUrlsAsync(eventId);
						if (urls.Any()) {
							return new ResolverResult("text/html", urls);
						}
					} catch (Exception ex) {
						Console.Error.WriteLine(ex);
					}
				}
			}

			var sidearm = Regex.Match(uri.PathAndQuery, @"/watch/\?Live=([0-9]+)&type=Live");
			if (sidearm.Success) {
				try {
					var urls = await SidearmSports.GetMediaUrlsAsync(uri, int.Parse(sidearm.Groups[1].Value));
					if (urls.Any()) {
						return new ResolverResult("text/html", urls);
					}
				} catch (Exception ex) {
					Console.Error.WriteLine(ex);
				}
			}

			async Task<ResolverResult> http(string method) {
				var req = WebRequest.CreateHttp(uri);
				req.Method = method;
				req.Accept = Program.Accept;
				req.UserAgent = Program.UserAgent;
				req.AllowAutoRedirect = false;
				req.CookieContainer = cookieContainer;
				using (var resp = await req.GetResponseAsync())
				using (var s = resp.GetResponseStream()) {
					int? code = (int?)(resp as HttpWebResponse)?.StatusCode;
					bool isHtml = new[] { "text/html", "application/xml+xhtml" }.Any(x => resp.ContentType.StartsWith(x));
					if (code >= 200 && code <= 300 && resp.ContentType.Contains("mpegurl")) {
						var req2 = WebRequest.CreateHttp(uri);
						req2.Method = "GET";
						req2.Accept = Program.Accept;
						req2.UserAgent = Program.UserAgent;
						req2.AllowAutoRedirect = false;
						req2.CookieContainer = cookieContainer;
						using (var resp2 = await req2.GetResponseAsync())
						using (var sr2 = new StreamReader(resp2.GetResponseStream())) {
							string contents = await sr2.ReadToEndAsync();
							if (contents.Contains("#EXT-X-MEDIA-SEQUENCE") || contents.Contains("#EXT-X-STREAM-INF")) {
								// HLS
								return new ResolverResult(resp.ContentType);
							} else {
								// Other playlist
								return new ResolverResult(resp.ContentType, await Disambiguation.ParseM3uAsync(contents));
							}
						}
					} else if (code == 300 && isHtml) {
						return new ResolverResult(resp.ContentType, await VideoUrlFinder.GetVideoUrisFromUriAsync(req.RequestUri, cookieContainer));
					} else if (code / 100 == 3) {
						// Redirect
						return new ResolverResult(resp.ContentType, new[] { new Uri(uri, resp.Headers["Location"]).AbsoluteUri });
					} else if (isHtml) {
						return new ResolverResult(resp.ContentType, await VideoUrlFinder.GetVideoUrisFromUriAsync(req.RequestUri, cookieContainer));
					} else {
						return new ResolverResult(resp.ContentType);
					}
				}
			}

			try {
				return await http("HEAD");
			} catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound) {
				return await http("GET");
			}
		}
	}
}
