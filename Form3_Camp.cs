using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net.Sockets;
using static HNC_EAmp.HNC_EAmp;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using algorithm;
using static alglib;
using System.Data.Common;
using System.Diagnostics;
using System.Media;
using System.CodeDom.Compiler;
using System.Reflection;

namespace Pseudo_RealTime_ALS_BCI
{
    public partial class Form3_Camp : Form
    {
        #region EAmp Library
        /// <summary>
        /// HNC_EAmp Library
        /// </summary>
        /// 
        private string ConnectState = "";
        private int sampling = 500;
        internal List<Chart> charts = new List<Chart>();
        internal List<Series> chartsSeries = new List<Series>();
        private Panel[] panels_signalQuality;
        private const int numChartUpdatePoint = 50;
        internal const int numChannel = 4;
        private const int numEegChannel = 3;
        internal List<List<double>> EegData_Raw = new List<List<double>>();
        internal List<List<double>> EegData_Filter = new List<List<double>>();
        private void button_connect_Click(object sender, EventArgs e)
        {
            displaySetting();
            ConnectState = InitEAmp(serialPort1, numChannel, EegData_Raw, EegData_Filter, numChartUpdatePoint,
            this.Controls[0], charts, chartsSeries, 3, panels_signalQuality);
            Command_ComStart(serialPort1);
            StartStorage();
        }
        internal string ReConnect2EAmp()
        {
            CloseEAmp(serialPort1);
            Thread.Sleep(1000);
            displaySetting();
            ConnectState = InitEAmp(serialPort1, 4, EegData_Raw, EegData_Filter, 25,
                    this.Controls[0], charts, chartsSeries, 3,
                    panels_signalQuality);
            Command_ComStart(serialPort1);
            StartStorage();
            return ConnectState;
        }

        private void dataStorageSetting()
        {
            for (int ich = 0; ich < numChannel; ich++)
            {
                EegData_Raw.Add(new List<double>());
                EegData_Filter.Add(new List<double>());
            }
        }
        private void displaySetting()
        {
            charts = new List<Chart>() { chartCH1, chartCH2, chartCH3, chartCH4 };
            panels_signalQuality = new Panel[numChannel] { panelCH1, panelCH2, panelCH3, panelCH4 };
        }
        private void dataClear()
        {
            EegData_Raw.Clear();
            EegData_Filter.Clear();
        }

        private void cutData(string state)
        {
            if (state == "async")
            {
                TriggerCode("async");
            }
        }
        #endregion
        #region sound effect
        SoundPlayer Bi_Player = new SoundPlayer("Voice/Bi.wav");
        SoundPlayer MA_Player = new SoundPlayer("Voice/MA.wav");
        SoundPlayer MI_Player = new SoundPlayer("Voice/MI.wav");
        SoundPlayer Rest_Player = new SoundPlayer("Voice/Resting.wav");
        SoundPlayer Answer_Player = new SoundPlayer("Voice/Answer.wav");
        #endregion
        public Form3_Camp()
        {
            InitializeComponent();
        }

        #region Two Sample Wilcoxon Signed Rank Test
        private double SignRank(double[] sample1, double[] sample2)
        {
            Accord.Statistics.Testing.TwoSampleWilcoxonSignedRankTest test =
                new Accord.Statistics.Testing.TwoSampleWilcoxonSignedRankTest(sample1, sample2);
            var pValue = test.PValue;
            return pValue;
            //Console.WriteLine(pValue);
        }
        #endregion

        private void Form3_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Space)
            {
                label_No.BackColor = Color.LightCoral;
            }
        }
    }
}
#region MA Evaluator (label text calculation)
// ------- Evaluator function -------
public class Evaluator
{
    public static object Eval(string statement)
    {
        return _evaluatorType.InvokeMember("Eval", BindingFlags.InvokeMethod, null, _evaluator, new object[] { statement });
    }

    static Evaluator()
    {
        CodeDomProvider provider = CodeDomProvider.CreateProvider("JScript");

        CompilerParameters parameters;
        parameters = new CompilerParameters();
        parameters.GenerateInMemory = true;

        CompilerResults results;
        results = provider.CompileAssemblyFromSource(parameters, _jscriptSource);

        Assembly assembly = results.CompiledAssembly;
        _evaluatorType = assembly.GetType("Evaluator");
        _evaluator = Activator.CreateInstance(_evaluatorType);
    }

    private static readonly object _evaluator;
    private static readonly Type _evaluatorType;

    private static readonly string _jscriptSource = @"class Evaluator { public function Eval(expr : String) : String { return eval(expr); } }";
}
#endregion
