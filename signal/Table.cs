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
    public partial class Table : Form
    {
        public Table()
        {
            InitializeComponent();
        }

        public List<double> QuantizationTable(List<double>Values, signal.Form1.Interval[] intervals)
        {
            List<double> QuantizedValues = new List<double>();
            for (int i = 0; i < Values.Count; i++)
            {
                int index = 0;
                double q = 0;
                for (int k = 0; k < intervals.Count(); k++)
                {
                    if (Values[i] >= intervals[k].Start && Values[i] <= intervals[k].End)
                    {
                        index = k;
                        q = intervals[k].MidPoint;
                        break;
                    }
                }
                QuantizedValues.Add(intervals[index].MidPoint);
                dataGridView1.Rows.Add(i, Values[i], index,intervals[index].code, intervals[index].MidPoint, q - Values[i]); 
            }
            return QuantizedValues;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
