using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace signal
{
    public partial class Filters : Form
    {
       
        public Filters()
        {
            InitializeComponent();
        }

        private void Filters_Load(object sender, EventArgs e)
        {
            label1.Text = "F1";
            label2.Text = "F2";
            label3.Text = "Cut Of Frequency";
            label4.Text = "Transition width";
            label5.Text = "StopBand attinuation";
            label6.Text = "Sampling Frequency";

            comboBox1.Items.AddRange(new string[] { "Filter", "Low Pass", "High Pass", "Band Pass", "Band Stop" });
            comboBox1.SelectedIndex = 0;

            textBox1.Text = "0";
            textBox2.Text = "0";
            textBox3.Text = "0";

            button1.Text = "Filter";

            label7.Text = "Up";
            label8.Text = "Down";

            textBox7.Text = "1";
            textBox8.Text = "1";

            button2.Text = "Sampling";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<double>Coeff = new List<double>(Coef());
            //StreamWriter SW = new StreamWriter("Coeff.txt");
            //foreach (double d in Coeff)
            //    SW.WriteLine(d);
            
            Form1 main = new Form1();

            DialogResult dr = this.openFileDialog1.ShowDialog();
            List<double> Signal = new List<double>();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader SR = new StreamReader(openFileDialog1.FileName);
                string line;

                while ((line = SR.ReadLine()) != null)
                    Signal.Add(double.Parse(line));

            }

            List<double> AfterConv = new List<double>(main.Convolution(Signal, Coeff));

            //SW = new StreamWriter("AfterConv.txt");
            //foreach (double d in AfterConv)
            //    SW.WriteLine(d);

            Result res = new Result();
            res.Add(Coeff, AfterConv);
            res.Show();
        }

        public List<double> Coef()
        {
            double F1 = double.Parse(textBox1.Text);
            double F2 = double.Parse(textBox2.Text);
            double CutOfFreq = double.Parse(textBox3.Text);
            double transition = double.Parse(textBox4.Text);
            double StopBandAttinuation = double.Parse(textBox5.Text);
            double SamplingFreq = double.Parse(textBox6.Text);

            List<double> W = new List<double>();
            List<double> H = new List<double>();

            double TransitionWidth = transition / SamplingFreq;

            int N = 0;
            if (StopBandAttinuation <= 21)
            {
                N = (int)(0.9 / TransitionWidth);

                if (N % 2 == 0)
                    N++;

                for (int n = 0; n <= N / 2; n++)
                    W.Add(1);

            }
            else if (StopBandAttinuation <= 44)
            {
                N = (int)(3.1 / TransitionWidth);

                if (N % 2 == 0)
                    N++;

                for (int n = 0; n <= N / 2; n++)
                    W.Add(0.5 + (0.5 * Math.Cos((2 * Math.PI * n) / N)));

            }
            else if (StopBandAttinuation <= 53)
            {
                N = (int)(3.3 / TransitionWidth);

                if (N % 2 == 0)
                    N++;

                for (int n = 0; n <= N / 2; n++)
                    W.Add(0.54 + (0.46 * Math.Cos((2 * Math.PI * n) / N)));

            }
            else if (StopBandAttinuation <= 74)
            {
                N = (int)(5.5 / TransitionWidth);

                if (N % 2 == 0)
                    N++;

                for (int n = 0; n <= N / 2; n++)
                    W.Add(0.42 + (0.5 * Math.Cos((2 * Math.PI * n) / (N - 1))) + (0.08 * Math.Cos((4 * Math.PI * n) / (N - 1))));

            }

            if (comboBox1.SelectedIndex == 1)
            {
                CutOfFreq = (CutOfFreq + (transition / 2)) / SamplingFreq;
                double Omega = 2 * Math.PI * CutOfFreq;

                H.Add(2 * CutOfFreq);
                for (int n = 1; n <= N / 2; n++)
                    H.Add((2 * CutOfFreq * Math.Sin(n * Omega)) / (n * Omega));
            }
            else if (comboBox1.SelectedIndex == 2)
            {
                CutOfFreq = (CutOfFreq + (transition / 2)) / SamplingFreq;
                double Omega = 2 * Math.PI * CutOfFreq;

                H.Add(1 - (2 * CutOfFreq));
                for (int n = 1; n <= N / 2; n++)
                    H.Add((-2 * CutOfFreq * Math.Sin(n * Omega)) / (n * Omega));
            }
            else if (comboBox1.SelectedIndex == 3)
            {
                F1 = (F1 - (transition / 2)) / SamplingFreq;
                double Omega1 = 2 * Math.PI * F1;

                F2 = (F2 + (transition / 2)) / SamplingFreq;
                double Omega2 = 2 * Math.PI * F2;

                H.Add(2 * (F2 - F1));
                for (int n = 1; n <= N / 2; n++)
                    H.Add(((2 * F2 * Math.Sin(n * Omega2)) / (n * Omega2)) -
                        ((2 * F1 * Math.Sin(n * Omega1)) / (n * Omega1)));
            }
            else if (comboBox1.SelectedIndex == 4)
            {
                F1 = (F1  +(transition / 2)) / SamplingFreq;
                double Omega1 = 2 * Math.PI * F1;

                F2 = (F2 - (transition / 2)) / SamplingFreq;
                double Omega2 = 2 * Math.PI * F2;

                H.Add(1 - (2 * (F2 - F1)));
                for (int n = 1; n <= N / 2; n++)
                    H.Add(((2 * F1 * Math.Sin(n * Omega1)) / (n * Omega1)) -
                        ((2 * F2 * Math.Sin(n * Omega2)) / (n * Omega2)));
            }
            List<double> Coeff = new List<double>();
            for (int i = 0; i < H.Count; i++)
                Coeff.Add(H[i] * W[i]);

            return Coeff;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog1.ShowDialog();
            List<double> Coeff = new List<double>(Coef());
            List<double> Values = new List<double>();
            List<double> result = new List<double>();
            List<double> filteredresulet = new List<double>();
            double up = double.Parse(textBox7.Text);
            double down = double.Parse(textBox8.Text);
          
            Form1 main = new Form1();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                    StreamReader SR = new StreamReader(openFileDialog1.FileName);
                    Result res = new Result();
                StreamWriter SW = new StreamWriter("1.Coeff.txt");
                    foreach (double d in Coeff)
                        SW.WriteLine(d);

                string line;
                    while ((line = SR.ReadLine()) != null)
                        Values.Add(double.Parse(line));
                    SR.Close();

                    //upsampling 
                        for (int i = 0; i < Values.Count; i++)
                        {
                            result.Add(Values[i]);
                            for (int j = 0; j < up - 1; j++)
                                result.Add(0);
                        }
                        SW = new StreamWriter("2.Up Sampling.txt");
                        foreach (double d in result)
                            SW.WriteLine(d);

                    filteredresulet = new List<double>(main.Convolution(Coeff, result));

                    SW = new StreamWriter("3.After Filtering.txt");
                    foreach (double d in filteredresulet)
                        SW.WriteLine(d);

                //downsampling
                      List<double>  result2 = new List<double>();
                        for (int i = 0; i < filteredresulet.Count; i++)
                        {
                            result2.Add(filteredresulet[i]);
                            for (int j = 0; j < down - 1; j++)
                                i++;
                        }

                        //SW = new StreamWriter("4.Down Sampling.txt");
                        //foreach (double d in result)
                        //    SW.WriteLine(d);
                        res.Add(result, result2);
                        res.Show();
                }

            }

        private void button2_Click_1(object sender, EventArgs e)
        {

            DialogResult dr = this.openFileDialog1.ShowDialog();
            List<double> Coeff = new List<double>(Coef());
            List<double> Values = new List<double>();
            List<double> result = new List<double>();
            List<double> filteredresulet = new List<double>();
            double up = double.Parse(textBox7.Text);
            double down = double.Parse(textBox8.Text);

            Form1 main = new Form1();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader SR = new StreamReader(openFileDialog1.FileName);
                Result res = new Result();
                StreamWriter SW = new StreamWriter("1.Coeff.txt");
                foreach (double d in Coeff)
                    SW.WriteLine(d);

                string line;
                while ((line = SR.ReadLine()) != null)
                    Values.Add(double.Parse(line));
                SR.Close();

                //upsampling 
                for (int i = 0; i < Values.Count; i++)
                {
                    result.Add(Values[i]);
                    for (int j = 0; j < up - 1; j++)
                        result.Add(0);
                }
                SW = new StreamWriter("2.Up Sampling.txt");
                foreach (double d in result)
                    SW.WriteLine(d);

                filteredresulet = new List<double>(main.Convolution(Coeff, result));

                SW = new StreamWriter("3.After Filtering.txt");
                foreach (double d in filteredresulet)
                    SW.WriteLine(d);

                //downsampling
                List<double> result2 = new List<double>();
                for (int i = 0; i < filteredresulet.Count; i++)
                {
                    result2.Add(filteredresulet[i]);
                    for (int j = 0; j < down - 1; j++)
                        i++;
                }

                //SW = new StreamWriter("4.Down Sampling.txt");
                //foreach (double d in result)
                //    SW.WriteLine(d);
                res.Add(result, result2);
                res.Show();
            }

        }


        }
        

    }

