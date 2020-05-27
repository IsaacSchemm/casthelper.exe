using System;
using System.Linq;
using System.Windows.Forms;

namespace CastHelper {
	static class Program {
		internal static string Accept = "application/vnd.apple.mpegurl,video/*,audio/*";
		internal static string UserAgent = "CastHelper/2.0 (https://github.com/IsaacSchemm/casthelper.exe)";

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1() {
				Url = args.FirstOrDefault() ?? ""
			});
		}
	}
}
