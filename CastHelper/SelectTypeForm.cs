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
		public MediaType SelectedType => (MediaType)comboBox1.SelectedItem;

		public SelectTypeForm() {
			InitializeComponent();
			comboBox1.Items.Clear();
			foreach (var t in Enum.GetValues(typeof(MediaType))) {
				comboBox1.Items.Add((MediaType)t);
			}
			if (comboBox1.SelectedIndex == -1) comboBox1.SelectedIndex = 0;
		}
	}
}
