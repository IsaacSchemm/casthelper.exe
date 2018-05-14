using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	static class Program {
		internal static string Accept = "application/vnd.apple.mpegurl,application/x-mpegurl,application/dash+xml,application/vnd.ms-sstr+xml,video/*,audio/*";
		internal static string UserAgent = "CastHelper/1.2 (https://github.com/IsaacSchemm/casthelper.exe)";

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
