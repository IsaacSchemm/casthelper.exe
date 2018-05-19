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
	public partial class SelectForm<T> : Form {
		public T SelectedItem => (T)comboBox1.SelectedItem;

		public SelectForm(string label, IEnumerable<T> items) {
			InitializeComponent();
			label1.Text = label;
			comboBox1.Items.Clear();
			foreach (var i in items) {
				comboBox1.Items.Add(i);
			}
			if (comboBox1.SelectedIndex == -1) comboBox1.SelectedIndex = 0;
		}
	}
}
