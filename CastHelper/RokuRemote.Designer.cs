namespace CastHelper {
	partial class RokuRemote {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.btnInstantReplay = new System.Windows.Forms.Button();
            this.btnRewind = new System.Windows.Forms.Button();
            this.btnPlayPause = new System.Windows.Forms.Button();
            this.btnFastForward = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnInstantReplay
            // 
            this.btnInstantReplay.Font = new System.Drawing.Font("Wingdings 3", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnInstantReplay.Location = new System.Drawing.Point(28, 12);
            this.btnInstantReplay.Name = "btnInstantReplay";
            this.btnInstantReplay.Size = new System.Drawing.Size(40, 40);
            this.btnInstantReplay.TabIndex = 2;
            this.btnInstantReplay.Text = "Q";
            this.btnInstantReplay.UseVisualStyleBackColor = true;
            this.btnInstantReplay.Click += new System.EventHandler(this.btnInstantReplay_Click);
            // 
            // btnRewind
            // 
            this.btnRewind.Font = new System.Drawing.Font("Webdings", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnRewind.Location = new System.Drawing.Point(74, 12);
            this.btnRewind.Name = "btnRewind";
            this.btnRewind.Size = new System.Drawing.Size(40, 40);
            this.btnRewind.TabIndex = 3;
            this.btnRewind.Text = "7";
            this.btnRewind.UseVisualStyleBackColor = true;
            this.btnRewind.Click += new System.EventHandler(this.btnRewind_Click);
            // 
            // btnPlayPause
            // 
            this.btnPlayPause.Font = new System.Drawing.Font("Webdings", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnPlayPause.Location = new System.Drawing.Point(120, 12);
            this.btnPlayPause.Name = "btnPlayPause";
            this.btnPlayPause.Size = new System.Drawing.Size(40, 40);
            this.btnPlayPause.TabIndex = 0;
            this.btnPlayPause.Text = ";";
            this.btnPlayPause.UseVisualStyleBackColor = true;
            this.btnPlayPause.Click += new System.EventHandler(this.btnPlayPause_Click);
            // 
            // btnFastForward
            // 
            this.btnFastForward.Font = new System.Drawing.Font("Webdings", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnFastForward.Location = new System.Drawing.Point(166, 12);
            this.btnFastForward.Name = "btnFastForward";
            this.btnFastForward.Size = new System.Drawing.Size(40, 40);
            this.btnFastForward.TabIndex = 1;
            this.btnFastForward.Text = "8";
            this.btnFastForward.UseVisualStyleBackColor = true;
            this.btnFastForward.Click += new System.EventHandler(this.btnFastForward_Click);
            // 
            // RokuRemote
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 64);
            this.Controls.Add(this.btnFastForward);
            this.Controls.Add(this.btnPlayPause);
            this.Controls.Add(this.btnRewind);
            this.Controls.Add(this.btnInstantReplay);
            this.Name = "RokuRemote";
            this.Text = "Cast Helper";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RokuRemote_FormClosing);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnInstantReplay;
		private System.Windows.Forms.Button btnRewind;
		private System.Windows.Forms.Button btnPlayPause;
		private System.Windows.Forms.Button btnFastForward;
	}
}