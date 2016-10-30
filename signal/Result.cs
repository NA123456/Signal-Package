using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace signal
{
    public partial class Result : Form
    {
        public Result()
        {
            InitializeComponent();
        }

        private void Result_Load(object sender, EventArgs e)
        {
            label1.Text = "NonNormalized";
            label2.Text = "Normalized";
        }

        public void Add(List<double> NonNormalized, List<double> Normalized)
        {
            for (int i = 0; i < NonNormalized.Count; i++)
                listBox1.Items.Add(NonNormalized[i]);
            for (int i = 0; i < Normalized.Count; i++)
                listBox2.Items.Add(Normalized[i]);
        
        }

        public void Res(List<double> Values)
        {
            for (int i = 0; i < Values.Count; i++)
                listBox1.Items.Add(Values[i]);
        }
    }
}
