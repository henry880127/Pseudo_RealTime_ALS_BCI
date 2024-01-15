using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pseudo_RealTime_ALS_BCI
{
    public partial class Form2_Paradigm : Form
    {
        #region Sockets(client)
        private Socket clientSocket;
        #endregion

        #region Timers
        public System.Timers.Timer timer_Cue = new System.Timers.Timer(3000);
        public System.Timers.Timer timer_TrainRest = new System.Timers.Timer(5000);
        public System.Timers.Timer timer_TrainMA = new System.Timers.Timer(5000);
        #endregion

        #region sound effect
        SoundPlayer Bi_Player = new SoundPlayer("Voice/Bi.wav");
        SoundPlayer MA_Player = new SoundPlayer("Voice/MA.wav");
        SoundPlayer MI_Player = new SoundPlayer("Voice/MI.wav");
        SoundPlayer Rest_Player = new SoundPlayer("Voice/Resting.wav");
        SoundPlayer Answer_Player = new SoundPlayer("Voice/Answer.wav");
        #endregion

        public Form2_Paradigm()
        {
            InitializeComponent();

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2575), requestCallBack,null);
            //clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2575));
            timer_Cue.Elapsed += new System.Timers.ElapsedEventHandler(timer_Cue_Elapsed);
            timer_TrainRest.Elapsed += new System.Timers.ElapsedEventHandler(timer_TrainRest_Elapsed);
            timer_TrainMA.Elapsed += new System.Timers.ElapsedEventHandler(timer_TrainMA_Elapsed);

            construct_ArrTask();
            Cue_start();
        }
        private void receiveCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("form2:receiveCallback!");
                clientSocket.EndReceive(ar);
                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), null);
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void requestCallBack(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine();
                clientSocket.EndConnect(ar);
                byte[] buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void sendTriggerCode(string trigger)
        {
            Console.WriteLine("form2:sendTriggerCode! Message:" + trigger);
            byte[] buffer1 = Encoding.ASCII.GetBytes(trigger);
            clientSocket.BeginSend(buffer1, 0, buffer1.Length, SocketFlags.None, new AsyncCallback(SendCallBack), null);
        }
        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("form2:SendCallBack!");
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Rest_start()
        {
            Bi_Player.Play();
            sendTriggerCode("rest");
            BeginInvoke(new UpdateUI(ControlUI), true);
            Console.WriteLine("Rest start!");
            timer_TrainRest.Start();
        }
        private void timer_TrainRest_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer_TrainRest.Stop();
            Cue_start();
        }
        private void MA_start()
        {
            Bi_Player.Play();
            sendTriggerCode("MA");
            BeginInvoke(new del_AssignValue(MA_AssignValue), 5);
            BeginInvoke(new del_ControlLabel(ControlLabel), label_value1, true);
            BeginInvoke(new del_ControlLabel(ControlLabel), label_value2, true);
            //MA_AssignValue(6); //level 6:prime numbers
            BeginInvoke(new UpdateUI(ControlMAPanel), true);
            Console.WriteLine("MA start!");
            timer_TrainMA.Start();

        }
        private void timer_TrainMA_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            BeginInvoke(new del_ControlLabel(ControlLabel), label_value1, false);
            BeginInvoke(new del_ControlLabel(ControlLabel), label_value2, false);
            timer_TrainMA.Stop();
            Cue_start();
        }
        private void Cue_start()
        {
            if (numTrained < 20)
            {
                stat_task = arrTask[numTrained];
                
                switch (stat_task)
                {
                    case "MA":
                        MA_Player.Play();
                        break;
                    case "rest":
                        Rest_Player.Play();
                        break;
                }
            }
            Console.WriteLine("Cue start!");
            timer_Cue.Start();
        }
        private void timer_Cue_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer_Cue.Stop();
            if (numTrained < 20)
            {
                numTrained++;
                Console.WriteLine("numTrained:" + numTrained);
                switch (stat_task)
                {
                    case "MA":
                        MA_start();
                        break;
                    case "rest":
                        Rest_start();
                        break;
                }
                Console.WriteLine("timer_Cue.Stop();");
            }
            else
            {
                Console.WriteLine("Training over!");
                sendTriggerCode("over");
                Console.WriteLine("clientSocket.Shutdown....");
                Console.WriteLine("this.Close();");
                Thread.Sleep(3000);
                // 參考:https://www.cnblogs.com/jshchg/p/12935039.html
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                CloseForm();
            }
        }
        private delegate void del_CloseForm();
        private void CloseForm()
        {
            if (this.InvokeRequired)
            {
                del_CloseForm del = new del_CloseForm(CloseForm);
                this.Invoke(del);
            }
            else
            {
                this.Close();
            }

        }


        #region Construct Task sequence
        private string[] arrTask = new string[20];
        private int numTrained = 0;
        private static string stat_task;
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
        public void construct_ArrTask()
        {
            setArr(ref arrTask, 10, "MA", 10, "rest");
            Shuffle(arrTask);
            foreach (var item in arrTask) Console.Write(item);
            Console.WriteLine();

        }
        public static void setArr(ref string[] arr, int nTarStim, string tar, int nStdStim, string std)
        {
            for (int i = 0; i < nTarStim; i++) arr[i] = tar;
            for (int i = nTarStim; i < nTarStim + nStdStim; i++) arr[i] = std;
        }
        #endregion

        #region MA
        private int[] arrPrm = new int[] { 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
        private string[] arrSign = { "+", "-" };
        private void MA_AssignValue(int level)
        {
            Random r = new Random();
            switch (level)
            {
                case 0:
                    label_value1.Text = Convert.ToString(0);
                    label_value2.Text = Convert.ToString(0);
                    break;
                case 1:
                    label_value1.Text = Convert.ToString(r.Next(1, 10));
                    label_value2.Text = Convert.ToString(r.Next(1, 10));
                    break;
                case 2:
                    label_value1.Text = Convert.ToString(r.Next(11, 50));
                    label_value2.Text = Convert.ToString(r.Next(11, 50));
                    break;
                case 3:
                    label_value1.Text = Convert.ToString(r.Next(51, 100));
                    label_value2.Text = Convert.ToString(r.Next(51, 100));
                    break;
                case 4:
                    label_value1.Text = Convert.ToString(r.Next(101, 500));
                    label_value2.Text = Convert.ToString(r.Next(101, 500));
                    break;
                case 5:
                    label_value1.Text = Convert.ToString(r.Next(501, 999));
                    label_value2.Text = Convert.ToString(r.Next(501, 999));
                    break;
                case 6:
                    label_value1.Text = Convert.ToString(arrPrm[r.Next(0, arrPrm.Length)]);
                    label_value2.Text = Convert.ToString(arrPrm[r.Next(0, arrPrm.Length)]);
                    label_symbol1.Text = Convert.ToString(arrSign[r.Next(0, arrSign.Length)]);
                    break;
            }
        }
        private void ControlLabel(System.Windows.Forms.Label label, bool onoff)
        {
            label.Visible = onoff;
        }
        private void ControlMAPanel(bool onoff)
        {
            tableLayoutPanel2.Visible = onoff;
        }
        private void ControlUI(bool onoff)
        {
            label_symbol1.Visible = onoff;
        }
        #endregion

        #region delegate
        delegate void UpdateUI(bool onoff);  // 利用委派更新GUI
        delegate void del_AssignValue(int level);  // 利用委派更新GUI
        delegate void del_ControlLabel(System.Windows.Forms.Label label, bool onoff);
        #endregion

    }
}
