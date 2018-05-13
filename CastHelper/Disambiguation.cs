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
	public static class Disambiguation {
		public static async Task<string> DisambiguateAsync(Uri uri, CookieContainer cookieContainer = null) {
			// Retrieve and display the HTML content.
			var req = WebRequest.CreateHttp(uri);
			req.Method = "GET";
			req.Accept = Program.Accept;
			req.UserAgent = Program.UserAgent;
			req.AllowAutoRedirect = false;
			if (cookieContainer != null) {
				req.CookieContainer = cookieContainer;
			}
			using (var resp = await req.GetResponseAsync())
			using (var sr = new StreamReader(resp.GetResponseStream()))
			using (var f = new Form {
				Width = 500,
				Height = 400
			}) using (var w = new WebBrowser {
				Dock = DockStyle.Fill,
				ScriptErrorsSuppressed = true
			}) {
				string html = await sr.ReadToEndAsync();
				html = new Regex("</head>", RegexOptions.IgnoreCase).Replace(
					html,
					$"<base href='{req.RequestUri}' /></head>");

				f.Controls.Add(w);
				f.Load += async (o, ea) => {
					w.Navigate("about:blank");
					while (w.Document?.Body == null) await Task.Delay(250);
					w.DocumentText = html;
				};
				string url = null;
				w.Navigating += (o, ea) => {
					if (ea.Url.AbsoluteUri != "about:blank") {
						url = ea.Url.AbsoluteUri;
						f.DialogResult = DialogResult.OK;
						f.Close();
					}
				};
				return f.ShowDialog() == DialogResult.OK
					? url
					: null;
			}
		}
	}
}
