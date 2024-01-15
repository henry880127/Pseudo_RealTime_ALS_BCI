using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media; // 語音
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using static HNC_EAmp.HNC_EAmp;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;
using algorithm;
using static alglib;
using System.Data.Common;
using System.Diagnostics;



namespace Pseudo_RealTime_ALS_BCI
{
    public partial class Form1 : Form
    {
        #region sockets
        public Socket serverSocket, clientSocket;
        private byte[] buffer;
        #endregion

        #region Variables
        private delegate void del_ShowCR_opt(string message);
        private int K_value = 3;
        /// <summary>
        /// training related
        /// </summary>
        private string[] arrTask = new string[20];
        private int numTrained = 0;

        private static string state;
        private static bool flag_Async = false;
        private static bool flag_Sync = false;

        private static string stat_task;
        /// <summary>
        /// settings
        /// </summary>
        private string ConnectState = "";
        private int sampling = 500;
        OnlineTrain onlineTrain = new OnlineTrain();
        SyncBCI syncBCI;

        /// <summary>
        /// HNC_EAmp Library
        /// </summary>
        internal List<Chart> charts = new List<Chart>();
        internal List<Series> chartsSeries = new List<Series>();
        private Panel[] panels_signalQuality;
        private const int numChartUpdatePoint = 50;
        internal const int numChannel = 4;
        private const int numEegChannel = 3;
        internal List<List<double>> EegData_Raw = new List<List<double>>();
        internal List<List<double>> EegData_Filter = new List<List<double>>();

        SoundPlayer HowCanIHelp_Player = new SoundPlayer("Voice/HowCanIHelp.wav");
        #endregion

        #region Threading
        public Thread thread_onlineTrain;
        public Thread thread_AsynDetect;
        public Thread thread_SyncDetect;
        #endregion

        #region Sound Effect
        SoundPlayer Bi_Player = new SoundPlayer("Voice/Bi.wav");
        SoundPlayer MA_Player = new SoundPlayer("Voice/MA.wav");
        SoundPlayer MI_Player = new SoundPlayer("Voice/MI.wav");
        SoundPlayer Rest_Player = new SoundPlayer("Voice/Resting.wav");
        SoundPlayer Answer_Player = new SoundPlayer("Voice/Answer.wav");
        #endregion
        public Form1()
        {
            InitializeComponent();
            dataStorageSetting();
            //displaySetting();
            //ConnectState = InitEAmp(serialPort1, numChannel, EegData_Raw, EegData_Filter, numChartUpdatePoint,
            //this.Controls[0], charts, chartsSeries, 3, panels_signalQuality);
            //timer_Cue.Elapsed += new System.Timers.ElapsedEventHandler(timer_Cue_Elapsed);
            timer_AsyncDetection.Elapsed += new System.Timers.ElapsedEventHandler(timer_AsyncDetection_Elapsed);
            timer_SyncBCI_CD.Elapsed += new System.Timers.ElapsedEventHandler(timer_SyncBCI_CD_Elapsed);
            timer_SyncBCI_AcquireEEG.Elapsed += new System.Timers.ElapsedEventHandler(timer_SyncBCI_AcquireEEG_Elapsed);
            //timer_TrainRest.Elapsed += new System.Timers.ElapsedEventHandler(timer_TrainRest_Elapsed);
            //timer_TrainMA.Elapsed += new System.Timers.ElapsedEventHandler(timer_TrainMA_Elapsed);

            //tabControl1 = new TabControl();
            #region SignRank Test
            double[] sample1 = { 1, 2, 3, 4, 5 };
            double[] sample2 = { -10, -5, 5014, 223, 65 };
            var pValue = SignRank(sample1, sample2);
            Console.WriteLine("p-Value is:" + pValue);
            #endregion

            #region Linq try
            List<List<double>> data = new List<List<double>>();
            data.Add(new List<double>());
            data.Add(new List<double>());
            data.Add(new List<double>());
            data[0].Add(0.0);
            data[1].Add(1.0);
            data[1].Add(1.1);
            data[2].Add(2.0);
            data[2].Add(2.1);
            data[2].Add(2.2);

            List<double[]> ddd = new List<double[]>();
            ddd.Add(new double[3]);
            ddd.Add(new double[3]);
            ddd.Add(new double[4]);
            var dd123 = ddd.Take(1);
            Console.WriteLine("dd123:" + dd123);

            Console.WriteLine("data.Count:" + data.Count);
            Console.WriteLine("data[0].Count:" + data[0].Count);
            Console.WriteLine("data[1].Count:" + data[1].Count);
            Console.WriteLine("data[2].Count:" + data[2].Count);

            int[] numbers = { 1, 2, 3, 4, 5 };
            int[] aaa = { 0, 0, 0, 0, 5 };
            double[] a = { 4, 2, 3, 1, 4, 2 };
            var sorted = a.OrderBy(x => x).ToList();
            Console.Write("OrderBy:");
            foreach (int i in sorted) Console.Write(" " + i);
            Console.WriteLine();
            var indexes = a.Select(x => sorted.IndexOf(x));
            Console.Write("index:");
            foreach (int i in indexes) Console.Write(" " + i);
            Console.WriteLine();
            int k = 4;
            var result = numbers.Skip(3).ToArray();
            foreach (int i in result) Console.WriteLine("skip(3):" + i);
            result = numbers.Take(0).ToArray();
            foreach (int i in result) Console.WriteLine("Take(0):" + i);
            //Console.WriteLine("skip(3):" + result);
            result = numbers.Take(3).ToArray();
            foreach (int i in result) Console.WriteLine("Take(3):" + i);
            //Console.WriteLine("Take(3):" + result);
            result = numbers.Take(k + 1).Skip(k).ToArray();
            foreach (int i in result) Console.WriteLine("Take(k+1).Skip(k):" + i);
            //Console.WriteLine("Take(3).Skip(2):" + result);
            result = numbers.Where((_, i) => i != k).ToArray();
            foreach (int i in result) Console.WriteLine("remove(k)" + i);
            result = numbers.Where((_, i) => i == k).ToArray();
            //foreach (int i in result) Console.WriteLine("Take(0):" + i);
            #endregion
        }

        #region Timers
        public System.Timers.Timer timer_AsyncDetection = new System.Timers.Timer(500);
        public System.Timers.Timer timer_Cue = new System.Timers.Timer(2000);
        public System.Timers.Timer timer_TrainRest = new System.Timers.Timer(3000);
        public System.Timers.Timer timer_TrainMA = new System.Timers.Timer(3000);
        public System.Timers.Timer timer_SyncBCI_CD = new System.Timers.Timer(1000);
        public System.Timers.Timer timer_SyncBCI_AcquireEEG = new System.Timers.Timer(500);
        #endregion

        #region EAmp Library
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
            if(state == "async")
            {
                TriggerCode("async");
            }
        }
        #endregion

        #region Asynchronous BCI
        internal static int[] detectChs = new int[] { 0, 1, 2 };
        internal AsyncDetection asyncBCI;
        private void AsyncBCI_Start()
        {
            asyncBCI = new AsyncDetection(7);
            timer_AsyncDetection.Start();
            flag_Async = true;
            //dataClear();
            //for (int ich = 0; ich < numChannel; ich++)
            //{
            //    EegData_Raw.Add(new List<double>());
            //    EegData_Filter.Add(new List<double>());
            //}
            asyncBCI.OptConfig = onlineTrain.OptConfig;
        }
        private void timer_AsyncDetection_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (EegData_Filter[0].Count > 2500 & flag_Async == true)
            {
                Console.WriteLine("Async Detect!!");
                //AsynchronousDetection(EegData_Filter);
                thread_AsynDetect = new Thread(() => AsynchronousDetection(EegData_Filter));
                thread_AsynDetect.IsBackground = true;
                thread_AsynDetect.Start();
            }
            else if(flag_Sync)
            {
                Console.WriteLine("Already turned Async BCI on.");
                SyncBCI_Start();
                timer_AsyncDetection.Stop();
            }
        }
        private void AsynchronousDetection(List<List<double>> EegData_Filter)
        {
            if (flag_Async == true)
            {
                var CurrentData = CutAsyncData(EegData_Filter, detectChs, 2500); // 抓前三個channel用作判斷
                var cuurentFeatureVec =  asyncBCI.ExtractFeat_currentData(CurrentData);
                var tempResult = asyncBCI.classify(onlineTrain.OptFeatureVec_group1, onlineTrain.OptFeatureVec_group2, cuurentFeatureVec, K_value);
                // asyncBCI.classify(onlineTrain.OptFeatureVec_group1, onlineTrain.OptFeatureVec_group2, K_value);
                var votes = asyncBCI.Forward(Convert.ToInt32(tempResult));
                var asycDectResult = asyncBCI.Voting(votes);
                Console.Write("Attention votes:");
                foreach (var vote in asyncBCI.AttentionVotes) Console.Write(vote + " ");
                Console.WriteLine("\nVoting Result:" + asycDectResult + "\n");
                if (asycDectResult == "attention" & flag_Sync == false)
                {
                    flag_Async = false;
                    flag_Sync = true;
                    BeginInvoke(new Del_ControlUI(ControlUI), tabControl1, tabPage2);
                }
            }
            else Console.WriteLine("Already turned Async BCI on.");
        }

        private void button_AsyncDetectStart_Click(object sender, EventArgs e)
        {
            AsyncBCI_Start();
        }
        private double[,] CutAsyncData(List<List<double>> dataTotal, int[] channel, int samplingPoints)
        {
            int numChannel = channel.Length;
            double[,] tempData = new double[numChannel, samplingPoints];
            for (int i = 0; i < numChannel; i++)
            {
                for (int pts = 0; pts < samplingPoints; pts++)
                {
                    tempData[i, pts] = dataTotal[channel[i]][dataTotal[0].Count - 1 - samplingPoints + pts];
                }
            }
            return tempData;
        }

        #endregion

        #region Synchronous BCI
        static public int sec_CD, trigger_Time;
        public static string trigger_Sync = "Sync";
        private void SyncBCI_Start()
        {
            flag_Sync = true;
            Answer_Player.Play();
            trigger_Time = 0;
            sec_CD = 5;
            timer_SyncBCI_CD.Start();
            BeginInvoke(new Del_ControlUI(ControlUI), tabControl1, tabPage2);
        }
        private void timer_SyncBCI_CD_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sec_CD > 0)
            {
                BeginInvoke(new UpdateCD(Control_label), label_CountDown, sec_CD.ToString());
            }
            else if (sec_CD == 0)
            {
                Bi_Player.Play();
                Sync_AcquireEEG_Start();
                BeginInvoke(new UpdateCD(Control_label), label_CountDown, "+");
            }
            else if (sec_CD == -6)
            {
                //List<double[,]> data = ExtractDataEpoch(EegData_Filter, trigger_Sync, 2500);
                thread_SyncDetect = new Thread(() => SynchronousDetection());
                thread_SyncDetect.IsBackground = true;
                thread_SyncDetect.Start();
                
                timer_SyncBCI_CD.Stop();
            }
            sec_CD--;
        }
        private void Sync_AcquireEEG_Start()
        {
            syncBCI = new SyncBCI(onlineTrain.OptConfig, 5);
            timer_SyncBCI_AcquireEEG.Start();
            Console.WriteLine("Sync Trigger!!");
            TriggerCode(trigger_Sync);
            trigger_Time++;
        }
        private void timer_SyncBCI_AcquireEEG_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            //TriggerCode("detect");
            if (flag_Sync & trigger_Time < 3)
            {
                Console.WriteLine("Sync Trigger!!");
                trigger_Time++;
                TriggerCode(trigger_Sync);
            }
            else
            {
                //flag_Sync = false;
                timer_SyncBCI_AcquireEEG.Stop();
            }
        }

        // 參考https://ithelp.ithome.com.tw/articles/10257485
        // 參考https://blog.opasschang.com/understand-csharp-asyn/

        private void SynchronousDetection()
        {
            if (flag_Sync == true)
                {
                double Sync_result;
                try
                {
                    List<double[,]> data = ExtractDataEpoch(EegData_Filter, trigger_Sync, 2500);
                    syncBCI.CurrentDataList = data;
                }
                catch { }
                Sync_result = syncBCI.OnlineClassify(onlineTrain.OptFeatureVec_group1, onlineTrain.OptFeatureVec_group2, K_value);
                Console.WriteLine(Sync_result.ToString());
                flag_Sync = false;
                flag_Async = true;
                TriggerCodeClear();
                Thread.Sleep(1000);
                if (Sync_result == 2)
                {
                    BeginInvoke(new PressLabel(Press), label_Attention);
                    Thread.Sleep(500);
                    BeginInvoke(new PressLabel(CompletePress), label_Attention);
                    OpenUrl("https://www.youtube.com/watch?v=uSiHqxgE2d0&list=PL0KTvQTu1m_zFqIrErdsqNywdSgG7eeYy&index=1&ab_channel=RayCharles");
                }
                else if (Sync_result == 1)
                {
                    BeginInvoke(new PressLabel(Press), label_Rest);
                    Thread.Sleep(500);
                    BeginInvoke(new PressLabel(CompletePress), label_Rest);
                    OpenUrl("https://www.youtube.com/watch?v=DGXAY5Y3ZuU&ab_channel=NBCNews");
                }
                AsyncBCI_Start();
                BeginInvoke(new Del_ControlUI(ControlUI), tabControl1, tabPage1);
            }
        }
        
        // 參考網站:https://dotblogs.com.tw/shinli/2015/04/16/151076
        private void Control_label(Label label, string CD)
        {
            label.Text = CD;
        }
        delegate void UpdateCD(Label label, string CD);
        private void Visiblize_label_CD(Label label ,bool onoff) { label.Visible = onoff; }
        delegate void VisiblizeUI(Label label, bool onoff);
        private void ControlUI(TabControl tabControl, TabPage tabPage) { tabControl.SelectedTab = tabPage; }
        delegate void Del_ControlUI(TabControl tabControl, TabPage tabPage);
        private void Press(System.Windows.Forms.Label label)
        {
            label.BackColor = Color.DodgerBlue;
            label.BorderStyle = BorderStyle.Fixed3D;
        }
        private void CompletePress(System.Windows.Forms.Label label)
        {
            label.BackColor = Color.LightSkyBlue;
            label.BorderStyle = BorderStyle.FixedSingle;
        }
        delegate void PressLabel(System.Windows.Forms.Label label);

        private void VisiblePanel(TableLayoutPanel panel, bool onoff)
        {
            panel.Visible = onoff;
        }
        delegate void del_VisiblePanel(TableLayoutPanel panel, bool onoff);
        #endregion

        #region BCI APP
        /// <summary>
        /// C# Winform access to URL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        public void OpenUrl(string url)
        {
            Process process = new Process();
            process.StartInfo.FileName = "msedge.exe";
            process.StartInfo.Arguments = url;
            process.Start();
        }
        #endregion

        #region Train
        private void button_TrainStart_Click(object sender, EventArgs e)
        {
            onlineTrain = new OnlineTrain();
            Form2_Paradigm f2 = new Form2_Paradigm();
            f2.Show();
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2575));
                serverSocket.Listen(0);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch { }
        }
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("form1:Accepted!");
                clientSocket = serverSocket.EndAccept(ar);
                // 第一次會空收
                // 參考網址:https://www.itread01.com/article/1478742797.html
                // Convert the string data byte data using to ASCII encoding.
                byte[] buffer1 = Encoding.ASCII.GetBytes("hello client");
                // Convert the string data byte data using to ASCII encoding.
                Console.WriteLine("form1:Sending! Message:hello client");
                clientSocket.BeginSend(buffer1, 0, buffer1.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);

                buffer = new byte[clientSocket.ReceiveBufferSize];
                // Begin receiving the data from the remote device.  
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), null);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
            }
            catch { }
        }
        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("form1:SendCallBack!");
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                int count = clientSocket.EndReceive(ar);
                Array.Resize(ref buffer, count);
                string msg = Encoding.ASCII.GetString(buffer);
                Console.WriteLine("form1:receiveCallback! Message:" + msg);
                if (msg == "over")
                {
                    BeginInvoke(new del_VisiblePanel(VisiblePanel), tableLayoutPanel2, false);
                    BeginInvoke(new VisiblizeUI(Visiblize_label_CD), label4, true);
                    onlineTrain.TestSecond = 5;
                    onlineTrain.Data_group1 = ExtractDataEpoch(EegData_Filter, "rest", 2500);
                    onlineTrain.Data_group2 = ExtractDataEpoch(EegData_Filter, "MA", 2500);
                    Console.WriteLine("data_group1 length:" + onlineTrain.Data_group1.Count);

                    Console.WriteLine("data_group2 length:" + onlineTrain.Data_group2.Count);
                    onlineTrain.FeatureSelection(K_value);
                    showCR_opt("Trained model, CR:" + onlineTrain.CR_opt.ToString());
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    
                }
                else
                {
                    TriggerCode(msg);

                    buffer = new byte[clientSocket.ReceiveBufferSize];
                    // Begin receiving the data from the remote device.  
                    clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), null);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // 改到Socket 裡面執行了
        //private void OnlineTraing()  
        //{
        //    OnlineTrain onlineTrain = new OnlineTrain();
        //    onlineTrain.Data_group1 = ExtractDataEpoch(EegData_Filter, "rest", 1500);
        //    onlineTrain.Data_group2 = ExtractDataEpoch(EegData_Filter, "MA", 1500);
        //    Console.WriteLine("data_group1 length:" + onlineTrain.Data_group1.Count);
        //    Console.WriteLine("data_group2 length:" + onlineTrain.Data_group2.Count);
        //    onlineTrain.FeatureSelection();
        //    label1.Text = onlineTrain.CR_opt.ToString();
        //}
        private void showCR_opt(string Message)
        {
            if (this.InvokeRequired)
            {
                del_ShowCR_opt del = new del_ShowCR_opt(showCR_opt);
                this.Invoke(del, Message);
            }
            else
            {
                this.label1.Text = Message;
            }

        }
        public static void Shuffle<T>(T[] Source)
        {
            if (Source == null) return;
            int len = Source.Length;//用變數記會快一點點點
            Random rd = new Random();
            int r;//記下隨機產生的號碼
            T tmp;//暫存用
            for (int i = 0; i < len - 1; i++)
            {
                r = rd.Next(i, len);//取亂數，範圍從自己到最後，決定要和哪個位置交換，因此也不用跑最後一圈了
                if (i == r) continue;
                tmp = Source[i];
                Source[i] = Source[r];
                Source[r] = tmp;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SyncBCI_Start();
            BeginInvoke(new Del_ControlUI(ControlUI), tabControl1, tabPage2);
            //OpenUrl("https://www.youtube.com/watch?v=uSiHqxgE2d0&list=PL0KTvQTu1m_zFqIrErdsqNywdSgG7eeYy&index=1&ab_channel=RayCharles");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BeginInvoke(new del_VisiblePanel(VisiblePanel), tableLayoutPanel2, false);
            BeginInvoke(new VisiblizeUI(Visiblize_label_CD), label4, true);
            //ReConnect2EAmp();
        }

        public void construct_ArrTask()
        {
            setArr(ref arrTask, 3, "MA", 3, "rest");
            Shuffle(arrTask);
            foreach (var item in arrTask) Console.Write(item);
            Console.WriteLine();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form3_Camp f3 = new Form3_Camp();
            f3.Show();
        }

        public static void setArr(ref string[] arr, int nTarStim, string tar, int nStdStim, string std)
        {
            for (int i = 0; i < nTarStim; i++) arr[i] = tar;
            for (int i = nTarStim; i < nTarStim + nStdStim; i++) arr[i] = std;
        }
        #endregion

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
    }
}
