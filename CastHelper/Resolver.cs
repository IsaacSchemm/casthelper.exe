using System;
using System.Collections.Generic;
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

			var req = WebRequest.CreateHttp(uri);
			req.Method = "HEAD";
			req.Accept = Program.Accept;
			req.UserAgent = Program.UserAgent;
			req.AllowAutoRedirect = false;
			req.CookieContainer = cookieContainer;
			using (var resp = await req.GetResponseAsync())
			using (var s = resp.GetResponseStream()) {
				int? code = (int?)(resp as HttpWebResponse)?.StatusCode;
				bool isHtml = new[] { "text/html", "application/xml+xhtml" }.Any(x => resp.ContentType.StartsWith(x));
				if (code == 300 && resp.ContentType.StartsWith("audio/mpegurl")) {
					return new ResolverResult(resp.ContentType, await Disambiguation.ParseM3uAsync(req.RequestUri, cookieContainer));
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
	}
}
