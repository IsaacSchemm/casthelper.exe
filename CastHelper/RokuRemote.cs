using RokuDotNet.Client.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CastHelper {
	public partial class RokuRemote : Form {
		private readonly IRokuDeviceInput _roku;

		public Task LastTask { get; private set; } = Task.CompletedTask;
		
		public RokuRemote(IRokuDeviceInput roku) {
			_roku = roku;
			InitializeComponent();
		}

		private async void RokuKeyPress(SpecialKeys key) {
			if (_roku == null) return;
			try {
				await _roku.KeyPressAsync(key);
			} catch (Exception) {
				MessageBox.Show(this, "Could not connect to the Roku device.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnInstantReplay_Click(object sender, EventArgs e) {
			RokuKeyPress(SpecialKeys.InstantReplay);
		}
		
		private void btnRewind_Click(object sender, EventArgs e) {
			RokuKeyPress(SpecialKeys.Reverse);
		}

		private void btnPlayPause_Click(object sender, EventArgs e) {
			RokuKeyPress(SpecialKeys.Play);
		}

		private void btnFastForward_Click(object sender, EventArgs e) {
			RokuKeyPress(SpecialKeys.Forward);
		}
		
		private void RokuRemote_FormClosing(object sender, FormClosingEventArgs e) {
			var result = MessageBox.Show(this, "Would you like to stop video playback on the Roku?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			if (result == DialogResult.Cancel) e.Cancel = true;
			if (result == DialogResult.Yes) LastTask = _roku?.KeyPressAsync(SpecialKeys.Home);
		}
	}
}
