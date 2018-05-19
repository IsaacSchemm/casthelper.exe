﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CastHelper {
	public class ResolverResult {
		public readonly string ContentType;
		public readonly IEnumerable<string> Links;

		public ResolverResult(string contentType) {
			ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
			Links = Enumerable.Empty<string>();
		}

		public ResolverResult(IEnumerable<string> links) {
			ContentType = null;
			Links = links?.ToList() ?? throw new ArgumentNullException(nameof(links));
		}
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
							return new ResolverResult(urls);
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
				if (new[] {
						"text/html",
						"application/xml+xhtml"
					}.Any(x => resp.ContentType.StartsWith(x))) {
					return new ResolverResult(await VideoUrlFinder.GetVideoUrisFromUriAsync(req.RequestUri, cookieContainer));
				} else if (code == 300 && resp.ContentType.StartsWith("audio/mpegurl")) {
					return new ResolverResult((await Disambiguation.ParseM3uAsync(req.RequestUri, cookieContainer)).Select(p => p.Url));
				} else if (code / 100 == 3) {
					// Redirect
					return new ResolverResult(new[] { new Uri(uri, resp.Headers["Location"]).AbsoluteUri });
				} else {
					return new ResolverResult(resp.ContentType);
				}
			}
		}
	}
}
