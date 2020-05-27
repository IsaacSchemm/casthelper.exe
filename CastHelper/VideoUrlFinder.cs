using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public static class VideoUrlFinder {
		// This search ignores query strings
		private static readonly Regex videoUrlRegex1 = new Regex(@"['""`]((https?:)?//[^'""`]+\.(m3u8|mp4)(\?[^'""`]+)?)['""`]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static readonly Regex videoUrlRegex2 = new Regex(@"<video[^>]+src=['""]([^'""]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static readonly Regex iframeRegex = new Regex(@"<iframe[^>]+src=['""]([^'""]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		private static readonly Regex regularLinkRegex = new Regex(@"<a[^>]+href=['""]([^'""]+)[^>]+>+([^<]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		private static IEnumerable<PlaylistItem> GetUrlRegexMatches(Regex regex, string html) {
			foreach (Match m in regex.Matches(html)) {
				string url = m.Groups[1].Value;
				if (url.StartsWith("//")) url = $"http:{url}";

				string name = regex == regularLinkRegex
					? m.Groups[2].Value
					: url;

				yield return new PlaylistItem(WebUtility.HtmlDecode(name), WebUtility.HtmlDecode(url));
			}
		}

		public static async Task<PlaylistItem> GetVideoUriFromUriAsync(Uri uri, CookieContainer cookieContainer = null) {
			var urls = await GetVideoUrisFromUriAsync(uri, cookieContainer);
			if (urls.Count() > 1) {
				using (var f = new SelectForm<PlaylistItem>("Multiple possible video URLs were found.", urls)) {
					return f.ShowDialog() == DialogResult.OK
						? f.SelectedItem
						: null;
				}
			} else {
				return urls.SingleOrDefault();
			}
		}

		public static async Task<IEnumerable<PlaylistItem>> GetVideoUrisFromUriAsync(Uri uri, CookieContainer cookieContainer = null) {
			var req = WebRequest.CreateHttp(uri);
			req.Method = "GET";
			req.Accept = Program.Accept;
			req.UserAgent = Program.UserAgent;
			req.AllowAutoRedirect = false;
			if (cookieContainer != null) {
				req.CookieContainer = cookieContainer;
			}
			using (var resp = await req.GetResponseAsync())
			using (var sr = new StreamReader(resp.GetResponseStream())) {
				string html = await sr.ReadToEndAsync();
				return GetVideoUrisFromHtml(html);
			}
		}

		private static IEnumerable<T> DefaultListIfEmpty<T>(this IEnumerable<T> orig, IEnumerable<T> def) {
			using (var e = orig.GetEnumerator()) {
				if (e.MoveNext()) {
					do {
						yield return e.Current;
					} while (e.MoveNext());
				} else {
					foreach (var o in def) {
						yield return o;
					}
				}
			}
		}

		public static IEnumerable<PlaylistItem> GetVideoUrisFromHtml(string html) {
			return Enumerable.Empty<PlaylistItem>()
				.Concat(GetUrlRegexMatches(videoUrlRegex1, html))
				.Concat(GetUrlRegexMatches(videoUrlRegex2, html))
				.DefaultListIfEmpty(GetUrlRegexMatches(iframeRegex, html))
				.DefaultListIfEmpty(GetUrlRegexMatches(regularLinkRegex, html))
				.Distinct()
				.ToList();
		}
	}
}
