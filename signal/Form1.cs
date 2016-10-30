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
using ZedGraph;
using System.Numerics;

namespace signal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public struct Interval
        {
            public double Start;
            public double End;
            public double MidPoint;
            public string code;
        }

        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";

            //  Allow the user to select multiple images.
            this.openFileDialog1.Multiselect = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //PaintEventArgs Event  = new PaintEventArgs(null,Rectangle.Empty);
            //ControlPaint.DrawBorder(Event.Graphics, ClientRectangle, Color.Black, 5, ButtonBorderStyle.Solid,
            //    Color.Black, 5, ButtonBorderStyle.Solid, Color.Black, 5, ButtonBorderStyle.Solid, Color.Black, 5, ButtonBorderStyle.Solid);

            comboBox1.Items.AddRange(new string[] { "Transform", "DFT", "IDFT", "FFT", "IFFT" });
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.AddRange(new string[] { "Select", "Add", "Multiply", "Multiply by const", "Shift" });
            comboBox2.SelectedIndex = 0;

            comboBox3.Items.AddRange(new string[] { "Convolution", "Cross Correlation", "Direct Auto Correlation", "Fast CrossCorrelation", "Fast AutoCorrelation", "Fast Convolution" });
            comboBox3.SelectedIndex = 0;

            checkBox1.Text = "Folding";

            label4.Text = "Const: ";

            button1.Text = "Result";
            InitializeOpenFileDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<double> Res = new List<double>();

            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                zedGraphControl1.GraphPane.CurveList.Clear();
                zedGraphControl1.GraphPane.GraphObjList.Clear();

                zedGraphControl2.GraphPane.CurveList.Clear();
                zedGraphControl2.GraphPane.GraphObjList.Clear();

                int counter = 0;
                foreach (string filename in openFileDialog1.FileNames)
                {
                    StreamReader SR = new StreamReader(filename);
                    string line;
                    List<double> Values = new List<double>();
                    while ((line = SR.ReadLine()) != null)
                    {
                        Values.Add(double.Parse(line));
                    }
                    SR.Close();

                    if (comboBox2.SelectedIndex == 1)
                        Res = new List<double>(AddSignals(Values, Res));
                    else if (comboBox2.SelectedIndex == 2)
                        Res = new List<double>(MultiplySignals(Values, Res));
                    else if (comboBox2.SelectedIndex == 3)
                        Res = new List<double>(MultiplyByConst(Values, Res));
                    else if (comboBox2.SelectedIndex == 4)
                        shift(Values);
                    if (counter % 2 == 0)
                        DrawSignal(Values, Color.Blue, 1, zedGraphControl1);
                    else
                        DrawSignal(Values, Color.Aqua, 1, zedGraphControl1);
                    counter++;
                }

            }
            if(comboBox2.SelectedIndex !=4)
                DrawSignal(Res, Color.Red, 1, zedGraphControl2);

            StreamWriter sw = new StreamWriter("NewSignal.txt");
            foreach (double d in Res)
                sw.WriteLine(d);
            sw.Close();
        }

        public void shift(List<double> values)
        {
            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            double constant = double.Parse(textBox4.Text);
            GraphPane myPane = new GraphPane();
            myPane = zedGraphControl2.GraphPane;

            myPane.Title.Text = "Drawing Signal";

            myPane.XAxis.Title.Text = "time";
            myPane.YAxis.Title.Text = "voltage";


            PointPairList L = new PointPairList();
            if (checkBox1.Checked)
            {
                myPane.XAxis.Scale.Min = -1 * (values.Count - constant);
                myPane.XAxis.Scale.Max = constant;

                for (int i = values.Count - 1; i >= 0; i--)
                    L.Add((i - constant) * -1, values[i]);
            }
            else
            {
                myPane.XAxis.Scale.Min = -1 * constant;
                myPane.XAxis.Scale.Max = values.Count - constant;

                for (int i = 0; i < values.Count; i++)
                    L.Add((i - constant), values[i]);
            }
            var signalCurve = myPane.AddCurve("Signal", L, Color.Red, SymbolType.Circle);
            signalCurve.Line.IsVisible = false;

            //LineItem line = new LineItem(string.Empty, L, Color.Red, SymbolType.Circle);
            //line.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            //line.Line.Width = 1f;

            //myPane.CurveList.Add(line);

            //for (int i = 0; i < values.Count; i++)
            //{
            //    var line = new LineObj(Color.Red, i, values[i], i, 0);
            //    GraphPane pane = zedGraphControl2.GraphPane;
            //    pane.GraphObjList.Add(line);
            //}

            zedGraphControl2.AxisChange();
            zedGraphControl2.Refresh();

            //return values;
        }

        private void QuantizeSignale(List<double> Values)
        {
            double MinAmb = 999999999.0;
            double MaXAmb = -999999999.0;

            int NumOfLevels = 0;
            if (textBox1.Text != "")
                NumOfLevels = int.Parse(textBox1.Text);
            if (textBox2.Text != "")
                NumOfLevels = (int)Math.Pow(2, int.Parse(textBox2.Text));
            foreach (double v in Values)
            {
                if (v < MinAmb)
                    MinAmb = v;
                if (v > MaXAmb)
                    MaXAmb = v;
            }

            double delta = (MaXAmb - MinAmb) / NumOfLevels;

            Interval[] intervals = new Interval[NumOfLevels];
            intervals[0].Start = MinAmb;
            intervals[0].End = MinAmb + delta;
            intervals[0].MidPoint = (intervals[0].Start + intervals[0].End) / 2;

            if (textBox2.Text != "")
                intervals[0].code = Convert.ToString(0, 2).PadLeft(int.Parse(textBox2.Text), '0');
            if (textBox1.Text != "")
                intervals[0].code = Convert.ToString(0, 2).PadLeft((int)Math.Log(double.Parse(textBox1.Text), 2), '0');

            for (int i = 1; i < NumOfLevels; i++)
            {
                intervals[i].Start = intervals[i - 1].End;
                intervals[i].End = intervals[i].Start + delta;
                intervals[i].MidPoint = (intervals[i].Start + intervals[i].End) / 2;

                if (textBox2.Text != "")
                    intervals[i].code = Convert.ToString(i, 2).PadLeft(int.Parse(textBox2.Text), '0');
                if (textBox1.Text != "")
                    intervals[i].code = Convert.ToString(i, 2).PadLeft((int)Math.Log(double.Parse(textBox1.Text), 2), '0');
            }

            Table T = new Table();
            List<double> QuantizedValues = new List<double>();
            QuantizedValues = T.QuantizationTable(Values, intervals);
            DrawSignal(QuantizedValues, Color.Red, 1, zedGraphControl2);
            T.Show();

            StreamWriter sw = new StreamWriter("QuantizedSignal.txt");
            foreach (double d in QuantizedValues)
                sw.WriteLine(d);
            sw.Close();

        }
        public void DrawSignal(List<double> Values, Color c, double Omega, ZedGraphControl zg)
        {
            GraphPane myPane = new GraphPane();
            myPane = zg.GraphPane;
            // myPane = zedGraphControl2.GraphPane;

            myPane.Title.Text = "Drawing Signal";

            myPane.XAxis.Title.Text = "time";
            myPane.YAxis.Title.Text = "voltage";

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = Values.Count * Omega;


            PointPairList L = new PointPairList();
            for (int i = 0; i < Values.Count; i++)
                L.Add(i * Omega, Values[i]);

            var signalCurve = myPane.AddCurve("Signal", L, c, SymbolType.Circle);
            signalCurve.Line.IsVisible = false;

            //for (int i = 0; i < Values.Count; i++)
            //{
            //    var line = new LineObj(Color.Red, i * Omega, Values[i], i * Omega, 0);
            //    GraphPane pane = zg.GraphPane;
            //    pane.GraphObjList.Add(line);
            //}

                zg.AxisChange();
            zg.Refresh();
        }
        private List<double> AddSignals(List<double> values, List<double> sum)
        {
            //StreamWriter SW = new StreamWriter("NewSignal.txt");
            if (sum.Count == 0)
            {
                for (int i = 0; i < values.Count; i++)
                    sum.Add(values[i]);
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                    sum[i] += values[i];
            }
            return sum;
        }

        private List<double> MultiplySignals(List<double> Values, List<double> Res)
        {

            if (Res.Count == 0)
            {
                for (int i = 0; i < Values.Count; i++)
                    Res.Add(Values[i]);
            }
            else
            {
                for (int i = 0; i < Values.Count; i++)
                    Res[i] *= Values[i];
            }
            return Res;
        }

        private List<double> MultiplyByConst(List<double> Values, List<double> Res)
        {
            double Const = double.Parse(textBox4.Text);
            for (int i = 0; i < Values.Count; i++)
                Res.Add(Values[i] * Const);
            return Res;
        }
    
        private void button2_Click(object sender, EventArgs e)
        {
            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader SR = new StreamReader(openFileDialog1.FileName);
                string line;
                List<double> Values = new List<double>();
                while ((line = SR.ReadLine()) != null)
                {
                    Values.Add(double.Parse(line));
                }
                SR.Close();
                DrawSignal(Values, Color.Blue, 1, zedGraphControl1);
                QuantizeSignale(Values);
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////

        //Euler Formula  --- e^xj =cosx + j sinx 
        public List<Tuple<double, double>> DFT(List<Tuple<double, double>> Input, List<Tuple<double, double>> Harmonics, double arg)
        {

            for (int k = 0; k < Input.Count; k++)
            {
                double real = 0;
                double imaginary = 0;
                for (int n = 0; n < Input.Count; n++)
                {

                    real += Math.Round((Input[n].Item1 * Math.Cos(k * n * arg))
                        - (Input[n].Item2 * Math.Sin(k * n * arg)), 4);

                    imaginary += Math.Round((Input[n].Item1 * Math.Sin(k * n * arg)) +
                        (Input[n].Item2 * Math.Cos(k * n * arg)), 4);
                }
                Harmonics.Add(new Tuple<double, double>(real, imaginary));
            }
            return Harmonics;
        }

        public List<double> GetAmplitude(List<Tuple<double, double>> Harmonics, List<double> Amp)
        {
            for (int i = 0; i < Harmonics.Count; i++)
                Amp.Add(Math.Sqrt(Math.Pow(Harmonics[i].Item2, 2) + Math.Pow(Harmonics[i].Item1, 2)));
            return Amp;
        }
        public List<double> GetPhase(List<Tuple<double, double>> Harmonics, List<double> Phases)
        {
            for (int i = 0; i < Harmonics.Count; i++)
            {
                double phase = Math.Atan(Harmonics[i].Item2 / Harmonics[i].Item1);
                if (Harmonics[i].Item1 < 0 && Harmonics[i].Item2 >= 0)
                    phase += Math.PI;
                else if (Harmonics[i].Item2 < 0)
                    phase -= (Math.PI);
                Phases.Add(phase);
            }
            return Phases;
        }

        ///////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///  FFT 
        /// 

        public Tuple<double, double> Butterfly(Tuple<double, double> FFT1, Tuple<double, double> FFT2, Tuple<double, double> W, string str)
        {

            double real = 0;
            double img = 0;

            if (str == "up")
            {
                real = FFT1.Item1 + FFT2.Item1 * W.Item1 - W.Item2 * FFT2.Item2;
                img = FFT1.Item2 + FFT2.Item1 * W.Item2 + W.Item1 * FFT2.Item2;
            }
            else if (str == "down")
            {
                real = FFT1.Item1 - FFT2.Item1 * W.Item1 + W.Item2 * FFT2.Item2;
                img = FFT1.Item2 - FFT2.Item1 * W.Item2 - W.Item1 * FFT2.Item2;
            }
            return new Tuple<double, double>(real, img);
        }
        public List<Tuple<double, double>> FFT(List<Tuple<double, double>> values, int N, List<Tuple<double, double>> Harmonics, string Str)
        {
            var Even = new List<Tuple<double, double>>();
            var Odd = new List<Tuple<double, double>>();

            var FFT1 = new List<Tuple<double, double>>();
            var FFT2 = new List<Tuple<double, double>>();

            if (N == 2)
            {
                Harmonics.Add(new Tuple<double, double>((values[0].Item1 + values[1].Item1), (values[0].Item2 + values[1].Item2)));
                Harmonics.Add(new Tuple<double, double>((values[0].Item1 - values[1].Item1), (values[0].Item2 - values[1].Item2)));
                return Harmonics;
            }
            else
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (i % 2 == 0)
                        Even.Add(new Tuple<double, double>(values[i].Item1, values[i].Item2));
                    else
                        Odd.Add(new Tuple<double, double>(values[i].Item1, values[i].Item2));
                }

                FFT1 = FFT(Even, Even.Count, FFT1, Str);
                FFT2 = FFT(Odd, Odd.Count, FFT2, Str);

                //Euler Formula  --- e^xj =cosx + j sinx 
                for (int i = 0; i < N; i++)
                    Harmonics.Add(new Tuple<double, double>(0, 0));

                for (int k = 0; k < FFT1.Count; k++)
                {
                    double real = 0;
                    double imagine = 0;
                    if (Str == "FFT")
                    {
                        real = Math.Cos((-2 * Math.PI * k) / N);
                        imagine = Math.Sin((-2 * Math.PI * k) / N);
                    }
                    else
                    {
                        real = Math.Cos((2 * Math.PI * k) / N);
                        imagine = Math.Sin((2 * Math.PI * k) / N);
                    }
                    var W = new Tuple<double, double>(real, imagine);

                    var up = Butterfly(FFT1[k], FFT2[k], W, "up");
                    var down = Butterfly(FFT1[k], FFT2[k], W, "down");

                    //if (Str == "FFT")
                    //{
                    Harmonics[k] = up;
                    Harmonics[k + (N / 2)] = down;
                    //}
                    //else
                    //{
                    //    var item1 = up.Item1 / N;
                    //    var item2 = up.Item2 / N;
                    //    Harmonics[k] = new Tuple<double,double>(item1,item2);
                    //    item1 = down.Item1 / N;
                    //    item2 = down.Item2  /N;
                    //    Harmonics[k + (N / 2)] = new Tuple<double,double>(item1,item2);
                    //}
                }
                return Harmonics;
            }
        }
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public List<Tuple<double, double>> ReadSamples(StreamReader SR)
        {
            var Values = new List<Tuple<double, double>>();
            string line = " ";
            while ((line = SR.ReadLine()) != null)
            {
                Values.Add(new Tuple<double, double>(double.Parse(line), 0));
            }
            SR.Close();
            return Values;
        }

        public void ReadPolarForm(StreamReader SR, ref List<double> Amp, ref List<double> Phases)
        {
            var Values = new List<Tuple<double, double>>();
            string line = " ";
            while ((line = SR.ReadLine()) != null)
            {
                string[] str = line.Split('\t');
                //  Values.Add(new Tuple<double, double>(double.Parse(str[0]), double.Parse(str[1])));
                Amp.Add(double.Parse(str[0]));
                Phases.Add(double.Parse(str[1]));
            }
            SR.Close();

        }

        private void DFT_button3_Click(object sender, EventArgs e)
        {
            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            DialogResult dr = this.openFileDialog1.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                List<Tuple<double, double>> harmonics = new List<Tuple<double, double>>();
                List<double> Amp = new List<double>();
                List<double> Phases = new List<double>();


                StreamReader SR = new StreamReader(openFileDialog1.FileName);
                //    string line;
                List<Tuple<double, double>> Values = new List<Tuple<double, double>>();

                /************************ DFT ****************************/
                if (comboBox1.SelectedIndex == 1)
                {
                    Values = ReadSamples(SR);

                    //start time of fourier transform
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    /*****************************************************/
                    double arg = (-2 * Math.PI) / Values.Count;
                    harmonics = DFT(Values, harmonics, arg);

                    Amp = GetAmplitude(harmonics, Amp);

                    Phases = GetPhase(harmonics, Phases);

                    //end time
                    watch.Stop();
                    /************/

                    double sampleFreq = double.Parse(textBox3.Text);

                    double Omega = (2 * Math.PI) / (Values.Count * (1 / sampleFreq));

                    //Amplitude Graph
                    DrawSignal(Amp, Color.Blue, Omega, zedGraphControl1);
                    DrawSignal(Phases, Color.Black, Omega, zedGraphControl2);

                    MessageBox.Show(watch.Elapsed.ToString());

                    StreamWriter sw = new StreamWriter("DFT.txt");
                    for (int i = 0; i < Amp.Count; i++)
                        sw.WriteLine(Amp[i] + "\t" + Phases[i]);
                    sw.Close();
                }

                else if (comboBox1.SelectedIndex == 2)
                {
                    ReadPolarForm(SR, ref Amp, ref Phases);

                    for (int i = 0; i < Amp.Count; i++)
                    {
                        //if (Phases[i] < 0)
                        //    Phases[i] += Math.PI;
                        double real = Math.Round(Amp[i] * Math.Cos(Phases[i]), 4);
                        double img = Math.Round(Amp[i] * Math.Sin(Phases[i]), 4);

                        Values.Add(new Tuple<double, double>(real, img));
                    }

                    harmonics = DFT(Values, harmonics, (2 * Math.PI) / Values.Count);

                    StreamWriter sw = new StreamWriter("IDFT.txt");
                    for (int i = 0; i < harmonics.Count; i++)
                    {
                        double item1 = harmonics[i].Item1 / harmonics.Count;
                        double item2 = harmonics[i].Item2 / harmonics.Count;

                        harmonics[i] = new Tuple<double, double>(item1, item2);
                        sw.WriteLine(harmonics[i].Item1);
                    }
                    sw.Close();
                }///end

                else if (comboBox1.SelectedIndex == 3)
                {
                    Values = ReadSamples(SR);

                    //start time of fourier transform
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    /*****************************************************/
                    double arg = (-2 * Math.PI) / Values.Count;
                    //  harmonics = DFT(Values, harmonics, arg);
                    harmonics = FFT(Values, Values.Count, harmonics, "FFT");

                    Amp = GetAmplitude(harmonics, Amp);

                    Phases = GetPhase(harmonics, Phases);

                    //end time
                    watch.Stop();
                    /************/

                    double sampleFreq = double.Parse(textBox3.Text);

                    double Omega = (2 * Math.PI) / (Values.Count * (1 / sampleFreq));

                    //Amplitude Graph
                    DrawSignal(Amp, Color.Blue, Omega, zedGraphControl1);
                    DrawSignal(Phases, Color.Black, Omega, zedGraphControl2);

                    MessageBox.Show(watch.Elapsed.ToString());

                    StreamWriter sw = new StreamWriter("FFT.txt");
                    for (int i = 0; i < Amp.Count; i++)
                        sw.WriteLine(Amp[i] + "\t" + Phases[i]);
                    sw.Close();
                }///end

                else if (comboBox1.SelectedIndex == 4)
                {
                    ReadPolarForm(SR, ref Amp, ref Phases);

                    for (int i = 0; i < Amp.Count; i++)
                    {
                        //if (Phases[i] < 0)
                        //    Phases[i] += Math.PI;
                        double real = Math.Round(Amp[i] * Math.Cos(Phases[i]), 4);
                        double img = Math.Round(Amp[i] * Math.Sin(Phases[i]), 4);

                        Values.Add(new Tuple<double, double>(real, img));
                    }

                    harmonics = FFT(Values, Values.Count, harmonics, "IFFT");

                    StreamWriter sw = new StreamWriter("IFFT.txt");
                    for (int i = 0; i < harmonics.Count; i++)
                    {
                        double item1 = Math.Round(harmonics[i].Item1 / harmonics.Count, 1);
                        double item2 = Math.Round(harmonics[i].Item2 / harmonics.Count, 4);

                        harmonics[i] = new Tuple<double, double>(item1, item2);
                        sw.WriteLine(harmonics[i].Item1 + "\t" + harmonics[i].Item2);
                    }
                    sw.Close();
                }///end

            }
        }
        /////////////////////////////////////////////////////////////////////

        private List<double> Correlation(List<double> X1, List<double> X2, string str)
        {
            int NumOfPoints = X1.Count;
            List<double> R = new List<double>();
            List<double> tmp_X2 = new List<double>(X2);
            if (X1.Count != X2.Count)
            {
                NumOfPoints = X1.Count + X2.Count - 1;
                for (int i = X1.Count; i < NumOfPoints; i++)
                    X1.Add(0);
                for (int i = tmp_X2.Count; i < NumOfPoints; i++)
                    tmp_X2.Add(0);
            }

            for (int i = 0; i < NumOfPoints; i++)
            {
                double sum = 0;
                for (int j = 0; j < NumOfPoints; j++)
                    sum += (X1[j] * tmp_X2[j]);

                R.Add(sum / NumOfPoints);

                double tmp = tmp_X2[0];
                tmp_X2.RemoveAt(0);
                if (str == "Periodic")
                    tmp_X2.Add(tmp);
                else
                    tmp_X2.Add(0);
            }

            return R;
        }



        private List<double> Fast(List<Tuple<double, double>> X1, List<Tuple<double, double>> X2, string Str)
        {
            int NumOfPoints = X1.Count;

            if (X1.Count != X2.Count)
            {
                NumOfPoints = X1.Count + X2.Count - 1;
                for (int i = X1.Count; i < NumOfPoints; i++)
                    X1.Add(new Tuple<double, double>(0, 0));
                for(int i=X2.Count;i<NumOfPoints;i++)
                    X2.Add(new Tuple<double,double>(0,0));
            }

            List<Tuple<double, double>> L1 = new List<Tuple<double, double>>();
            List<Tuple<double, double>> L2 = new List<Tuple<double, double>>();
            L1 = FFT(X1, X1.Count, L1, "FFT");
            L2 = FFT(X2, X2.Count, L2, "FFT");

            for (int i = 0; i < L1.Count; i++)
            {
                Tuple<double, double> T = new Tuple<double, double>(0, 0);
                if (Str == "Correlation")
                    T = new Tuple<double, double>(L1[i].Item1, L1[i].Item2 * -1);
                else
                    T = new Tuple<double, double>(L1[i].Item1, L1[i].Item2);
                double real = L2[i].Item1 * T.Item1 - L2[i].Item2 * T.Item2;
                double img = L2[i].Item1 * T.Item2 + L2[i].Item2 * T.Item1;

                //x1 * x2
                L1[i] = new Tuple<double, double>(real, img);
            }

            L2 = new List<Tuple<double, double>>();
            L2 = FFT(L1, L1.Count, L2, "IFFT");

            for(int i=0;i<L2.Count;i++)
            {
                double d = L2[i].Item1 / L2.Count;
                L2[i] = new Tuple<double, double>(d, 0);
            }

            List<double> Res = new List<double>();
            for (int i = 0; i < L2.Count; i++)
            {
                double tmp = 0;
                if (Str == "Correlation")
                    tmp = L2[i].Item1 / L2.Count;
                else
                    tmp = L2[i].Item1;
                Res.Add(tmp);
            }
            return Res;
        }

       
        public List<double> Convolution(List<double> signal1, List<double> signal2)
        {
            List<double> result = new List<double>();
            int numofpoints = signal1.Count + signal2.Count - 1;

            for (int i = 0; i < numofpoints; i++)
            {
                double sum = 0;
                for (int j = 0; j <= i; j++)
                {
                    if ((j < signal1.Count && j >= 0) && (i - j < signal2.Count && i - j >= 0))
                        sum += signal1[j] * signal2[i - j];
                }
                result.Add(sum);
            }
            return result;
        }

        private List<double> Normalization(List<double> X1, List<double> X2, List<double> R)
        {
            List<double> Normalized = new List<double>();

            double sum1 = 0;
            for (int i = 0; i < X1.Count; i++)
                sum1 += (Math.Pow(X1[i], 2));

            double sum2 = 0;
            for (int i = 0; i < X2.Count; i++)
                sum2 += (Math.Pow(X2[i], 2));

            double term = Math.Sqrt(sum1 * sum2) / R.Count;

            for (int i = 0; i < R.Count; i++)
                Normalized.Add(R[i] / term);

            return Normalized;
        }

        private void button3_Click(object sender, EventArgs e)
        {

            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl2.GraphPane.CurveList.Clear();
            zedGraphControl2.GraphPane.GraphObjList.Clear();

            
            List<double> Values1 = new List<double>();
            List<double> Values2 = new List<double>();
            List<double> Res = new List<double>();
            List<double> Normalized = new List<double>();


            DialogResult dr = this.openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {

                foreach (string filename in openFileDialog1.FileNames)
                {
                    StreamReader SR = new StreamReader(filename);
                    string line;

                    if (Values1.Count == 0)
                    {
                        while ((line = SR.ReadLine()) != null)
                            Values1.Add(double.Parse(line));
                    }

                    else
                    {
                        while ((line = SR.ReadLine()) != null)
                            Values2.Add(double.Parse(line));
                    }
                    SR.Close();
                }

                if (comboBox3.SelectedIndex == 0)
                    Res = Convolution(Values1, Values2);

                    /*Peridic cross Correlation*/
                else if (comboBox3.SelectedIndex == 1 && radioButton1.Checked)
                {
                    Res = Correlation(Values1, Values2, "Periodic");
                    Normalized = new List<double>(Normalization(Values1, Values2, Res));
                }

                    /*Periodic direct Auto Correlation*/
                else if (comboBox3.SelectedIndex == 2 && radioButton1.Checked)
                {
                    Values2 = new List<double>(Values1);
                    Res = Correlation(Values1, Values2, "Periodic");
                    Normalized = new List<double>(Normalization(Values1, Values1, Res));
                }

                /*NonPeridic cross Correlation*/
                else if (comboBox3.SelectedIndex == 1 && radioButton2.Checked)
                {
                    Res = Correlation(Values1, Values2, "NonPeriodic");
                    Normalized = new List<double>(Normalization(Values1, Values2, Res));
                }

                /*nNonPeriodic direct Auto Correlation*/
                else if (comboBox3.SelectedIndex == 2 && radioButton2.Checked)
                {
                    Values2 = new List<double>(Values1);
                    Res = Correlation(Values1, Values2, "NonPeriodic");
                    Normalized = new List<double>(Normalization(Values1, Values2, Res));
                }
                    /*fast cross correlation*/
                else if (comboBox3.SelectedIndex == 3)
                {
                    List<Tuple<double, double>> X1 = new List<Tuple<double, double>>();
                    List<Tuple<double, double>> X2 = new List<Tuple<double, double>>();

                    foreach (double d in Values1)
                        X1.Add(new Tuple<double, double>(d, 0));
                    foreach (double d in Values2)
                        X2.Add(new Tuple<double, double>(d, 0));
                    Res = Fast(X1, X2, "Correlation");
                    Normalized = new List<double>(Normalization(Values1, Values2, Res));
                }
                /* fast Auto correlation*/
                else if (comboBox3.SelectedIndex ==4)
                {
                    List<Tuple<double, double>> X1 = new List<Tuple<double, double>>();
                    List<Tuple<double, double>> X2 = new List<Tuple<double, double>>();
                    Values2 = Values1;
                    foreach (double d in Values1)
                        X1.Add(new Tuple<double, double>(d, 0));
                    X2 = X1;
                    Res = Fast(X1, X2, "Correlation");
                    Normalized = new List<double>(Normalization(Values1, Values2, Res));
                }
                else if (comboBox3.SelectedIndex == 5)
                {
                    List<Tuple<double, double>> X1 = new List<Tuple<double, double>>();
                    List<Tuple<double, double>> X2 = new List<Tuple<double, double>>();

                    foreach (double d in Values1)
                        X1.Add(new Tuple<double, double>(d, 0));
                    foreach (double d in Values2)
                        X2.Add(new Tuple<double, double>(d, 0));
                    Res = Fast(X1, X2, "Convolution");
                    Normalized = new List<double>(Normalization(Values1,Values2,Res));
                }

                StreamWriter SW = new StreamWriter("Res.txt");
                for (int i = 0; i < Res.Count; i++)
                    SW.WriteLine(Res[i]);
                SW.Close();
                DrawSignal(Res, Color.Blue, 1, zedGraphControl1);
                
                StreamWriter Normalize = new StreamWriter("Normalized.txt");
                for (int i = 0; i < Normalized.Count; i++)
                    Normalize.WriteLine(Normalized[i]);
                Normalize.Close();
                DrawSignal(Normalized, Color.Blue,1, zedGraphControl2);

                Result res = new Result();
                res.Add(Res, Normalized);
                res.Show();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
           
            Filters filters = new Filters();
            filters.Show();
            
           
        }

    }
}