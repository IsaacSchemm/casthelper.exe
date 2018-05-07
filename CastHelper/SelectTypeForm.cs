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
	public partial class SelectTypeForm : Form {
		public string SelectedType => comboBox1.SelectedItem?.ToString();

		public SelectTypeForm() {
			InitializeComponent();
			if (comboBox1.SelectedIndex == -1) comboBox1.SelectedIndex = 0;
		}
	}
}
