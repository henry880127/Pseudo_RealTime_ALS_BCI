using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Accord.Statistics;

namespace Pseudo_RealTime_ALS_BCI
{
    public class Algorithm
    {
        /// <summary>
        /// KFD evaluate
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double KFD_Function(double[] data)
        {
            List<double[]> sort_distance = new List<double[]>();
            int xlength = data.GetLength(0);
            double L = new double();
            double[] R = new double[xlength];
            double kfd = new double();
            double[,] KFD = new double[1, 1];
            int n = xlength - 1;
            double d;

            for (int i = 0; i < n; i++)
            {
                double Q = Math.Sqrt((1 + Math.Pow(data[i] - data[i + 1], 2)));
                L = L + Q;
            }

            for (int w = 1; w < xlength; w++)
            {
                R[w - 1] = Math.Sqrt(Math.Pow(w, 2) + Math.Pow(data[0] - data[w], 2));
            }
            sort_distance = sort(R);
            d = sort_distance[0][xlength - 1];
            kfd = Math.Log10(n) / (Math.Log10(d / L) + Math.Log10(n));
            //KFD[0, 0] = kfd;
            return kfd;
        }
        public static double KFD_Function(List<double> data)
        {
            List<double[]> sort_distance = new List<double[]>();
            int xlength = data.Count;
            double L = new double();
            double[] R = new double[xlength];
            double kfd = new double();
            double[,] KFD = new double[1, 1];
            int n = xlength - 1;
            double d;

            for (int i = 0; i < n; i++)
            {
                double Q = Math.Sqrt((1 + Math.Pow(data[i] - data[i + 1], 2)));
                L = L + Q;
            }

            for (int w = 1; w < xlength; w++)
            {
                R[w - 1] = Math.Sqrt(Math.Pow(w, 2) + Math.Pow(data[0] - data[w], 2));
            }
            sort_distance = sort(R);
            d = sort_distance[0][xlength - 1];
            kfd = Math.Log10(n) / (Math.Log10(d / L) + Math.Log10(n));
            //KFD[0, 0] = kfd;
            return kfd;
        }

        /// <summary>
        /// KNN
        /// </summary>
        /// <param name="eye_repeat_times"></param>
        /// <param name="ch"></param>
        /// <param name="C_Relax_BP"></param>
        /// <param name="C_MI_BP"></param>
        /// <returns></returns>
        public static double KNN(int eye_repeat_times, int ch, double[,] C_Relax_BP, double[,] C_MI_BP)
        {
            // eye_repeat_times 訓練次數 、 ch 通道數 、 train_num 訓練資料筆數
            int train_num = eye_repeat_times * 2;
            double[,] KNN_temp_BP = new double[train_num, ch];  // 創建 放鬆與想像 的BP

            // 合併 Relax 與 MI
            for (int i = 0; i < eye_repeat_times; i++)
            {
                for (int ii = 0; ii < ch; ii++)
                {
                    KNN_temp_BP[i, ii] = C_Relax_BP[i, ii];
                    KNN_temp_BP[i + eye_repeat_times, ii] = C_MI_BP[i, ii];
                }
            }

            // 算距離
            double[] distance = new double[train_num];
            double distance_temp = 0;
            List<double[]> sort_distance = new List<double[]>();
            double[] C_vote = new double[train_num]; // 最後投票結果

            for (int i_trial = 0; i_trial < train_num; i_trial++)
            {
                for (int i = 0; i < train_num; i++)
                {
                    distance_temp = norm(Row(KNN_temp_BP, i_trial), Row(KNN_temp_BP, i));
                    distance[i] = distance_temp;
                }

                sort_distance = sort(distance); // 排序過後的值 與 ind

                double[] vote = new double[3]; // k 取 3，只找前三個最近的資料點判斷
                for (int m = 1; m <= 3; m++)
                {
                    double vote_temp = sort_distance[1][m];
                    vote[m - 1] = vote_temp;
                }

                double[] label = new double[3];
                for (int label_count = 0; label_count <= 2; label_count++)
                {
                    if (vote[label_count] <= eye_repeat_times)
                    {
                        label[label_count] = 1;
                    }
                    else
                    {
                        label[label_count] = 2;
                    }
                }

                double vote_result = 0;
                for (int i_label = 0; i_label <= 2; i_label++)
                {
                    vote_result += label[i_label];
                }

                if (vote_result <= 4)
                {
                    C_vote[i_trial] = 1;
                }
                else
                {
                    C_vote[i_trial] = 2;
                }
            }

            double CR_temp = 0;
            double CR = 0;
            for (int i = 0; i < eye_repeat_times; i++)
            {
                if (C_vote[i] == 1)
                {
                    CR_temp += 1;
                }
            }

            for (int i = eye_repeat_times; i < train_num; i++)
            {
                if (C_vote[i] == 2)
                {
                    CR_temp += 1;
                }
            }
            CR = (double)(CR_temp / train_num) * 100;
            return CR;
        }

        /// <summary>
        /// KNN_online, training_data1 as "class-1", training_data2 is "class-2"
        /// </summary>
        /// <param name="eye_repeat_times"></param>
        /// <param name="ch"></param>
        /// <param name="training_data1"></param>
        /// <param name="training_data2"></param>
        /// <param name="Test_data"></param>
        /// <returns></returns>
        public static double KNN_Online(int eye_repeat_times, int ch, double[,] training_data1, double[,] training_data2, double[,] Test_data)
        {
            // eye_repeat_times 訓練次數 、 ch 通道數 、 train_num 訓練資料筆數
            int train_num = eye_repeat_times * 2;
            double[,] Alpha_temp_BP = new double[train_num, ch];  // 創建 放鬆與想像 的BP

            // 合併 Relax 與 MI
            for (int i = 0; i < eye_repeat_times; i++)
            {
                for (int ii = 0; ii < ch; ii++)
                {
                    Alpha_temp_BP[i, ii] = training_data1[i, ii];
                    Alpha_temp_BP[i + eye_repeat_times, ii] = training_data2[i, ii];
                }
            }

            // 算距離
            int test_length = Test_data.GetLength(0);
            double[] distance = new double[train_num];
            double distance_temp = 0;
            List<double[]> sort_distance = new List<double[]>();
            double C_vote = new double(); // 最後投票結果

            for (int i_trial = 0; i_trial < test_length; i_trial++)
            {
                for (int i = 0; i < train_num; i++)
                {
                    distance_temp = norm(Row(Test_data, i_trial), Row(Alpha_temp_BP, i));
                    distance[i] = distance_temp;
                }

                sort_distance = sort(distance); // 排序過後的值 與 ind

                double[] vote = new double[3]; // k 取 3，只找前三個最近的資料點判斷
                for (int m = 0; m < 3; m++)
                {
                    double vote_temp = sort_distance[1][m];
                    vote[m] = vote_temp;
                }

                double[] label = new double[3];
                for (int label_count = 0; label_count <= 2; label_count++)
                {
                    if (vote[label_count] <= eye_repeat_times)
                    {
                        label[label_count] = 1;
                    }
                    else
                    {
                        label[label_count] = 2;
                    }
                }

                double vote_result = 0;
                for (int i_label = 0; i_label <= 2; i_label++)
                {
                    vote_result += label[i_label];
                }

                if (vote_result <= 4)
                {
                    C_vote = 1;
                }
                else
                {
                    C_vote = 2;
                }
            }

            return C_vote;
        }
        public static double KNN_Online(List<List<double>> training_data1, List<List<double>> training_data2, List<List<double>> Test_data, int K_value)
        {
            // training_data1 is "class 1" //training_data2 is "class 2"
            // [[feat0][feat1][feat2]  instance-0
            //  [feat0][feat1][feat2]  instance-1
            //  [feat0][feat1][feat2]  instance-2
            //  [feat0][feat1][feat2]] instance-3

            int train_num = training_data1.Count + training_data2.Count;
            int dim = training_data1[0].Count;
            double[,] trainSet = new double[train_num, dim];  // Declare feature-set to  become training data set.

            // combine class-1 & class-2; Also construcy array of training data.
            for (int i = 0; i < training_data1[0].Count; i++)
            {
                for (int ii = 0; ii < dim; ii++)
                {
                    trainSet[i, ii] = training_data1[i][ii];
                    // trainSet[i + training_data1[0].Count, ii] = training_data2[i][ii];
                }
            }
            for (int i = 0; i < training_data2[0].Count; i++)
            {
                for (int ii = 0; ii < dim; ii++)
                {
                    trainSet[i + training_data1[0].Count, ii] = training_data2[i][ii];
                }
            }
            // Construct an array of test data.
            double[,] tstSet = List2array(Test_data);

            // norm evaluate
            int test_length = Test_data.Count;
            double[] distance = new double[train_num];
            double distance_temp = 0;
            List<double[]> sort_distance = new List<double[]>();
            double C_vote = new double(); // 最後投票結果

            for (int i_trial = 0; i_trial < test_length; i_trial++)
            {
                for (int i = 0; i < train_num; i++)
                {
                    distance_temp = norm(Row(tstSet, i_trial), Row(trainSet, i));
                    distance[i] = distance_temp;
                }

                sort_distance = sort(distance); // 排序過後的值 與 ind

                double[] vote = new double[K_value]; // k 取 3，只找前三個最近的資料點判斷
                for (int m = 0; m < K_value; m++)
                {
                    double vote_temp = sort_distance[1][m];
                    vote[m] = vote_temp;
                }

                double[] label = new double[K_value];
                for (int label_count = 0; label_count < K_value; label_count++)
                {
                    if (vote[label_count] <= training_data1[0].Count)
                    {
                        label[label_count] = 1;
                    }
                    else
                    {
                        label[label_count] = 2;
                    }
                }

                int freq1 = label.Count(n => (n == 1));
                int freq0 = label.Count(n => (n == 2));
                if (freq1 > freq0) C_vote = 1;
                else C_vote = 2;
            }

            return C_vote;
        }
        public static double KNN_Online(List<List<double>> training_data1, List<List<double>> training_data2, List<double> Test_data, int K_value)
        {
            // training_data1 is "class 1" //training_data2 is "class 2"
            // [[feat0][feat1][feat2]  instance-0
            //  [feat0][feat1][feat2]  instance-1
            //  [feat0][feat1][feat2]  instance-2
            //  [feat0][feat1][feat2]] instance-3

            int train_num = training_data1.Count + training_data2.Count;
            int dim = training_data1[0].Count;
            double[,] trainSet = new double[train_num, dim];  // Declare feature-set to  become training data set.

            // combine class-1 & class-2; Also construcy array of training data.
            for (int i = 0; i < training_data1.Count; i++)
            {
                for (int ii = 0; ii < dim; ii++)
                {
                    trainSet[i, ii] = training_data1[i][ii];
                    // trainSet[i + training_data1[0].Count, ii] = training_data2[i][ii];
                }
            }
            for (int i = 0; i < training_data2.Count; i++)
            {
                for (int ii = 0; ii < dim; ii++)
                {
                    trainSet[i + training_data1.Count, ii] = training_data2[i][ii];
                }
            }
            // Construct an array of test data.
            double[] tstSet = Test_data.ToArray();

            // norm evaluate
            int test_length = Test_data.Count;
            double[] distance = new double[train_num];
            double distance_temp = 0;
            List<double[]> sort_distance = new List<double[]>();
            double C_vote = new double(); // 最後投票結果

            for (int i_trial = 0; i_trial < test_length; i_trial++)
            {
                for (int i = 0; i < train_num; i++)
                {
                    distance_temp = norm(tstSet, Row(trainSet, i));
                    distance[i] = distance_temp;
                }

                sort_distance = sort(distance); // 排序過後的值 與 ind

                double[] vote = new double[K_value]; // k 取 3，只找前三個最近的資料點判斷
                for (int m = 0; m < K_value; m++)
                {
                    double vote_temp = sort_distance[1][m];
                    vote[m] = vote_temp;
                }

                double[] label = new double[K_value];
                for (int label_count = 0; label_count < K_value; label_count++)
                {
                    if (vote[label_count] <= training_data1[0].Count)
                    {
                        label[label_count] = 1;
                    }
                    else
                    {
                        label[label_count] = 2;
                    }
                }

                int freq1 = label.Count(n => (n == 1));
                int freq0 = label.Count(n => (n == 2));
                if (freq1 > freq0) C_vote = 1;
                else C_vote = 2;
            }

            return C_vote;
        }
        public static double KNN_Online(List<List<double>> training_Total, int[] label_Total, List<List<double>> Test_data, int K_value)
        {
            // training_Total:
            // [[feat0][feat1][feat2]  instance-0
            //  [feat0][feat1][feat2]  instance-1
            //  [feat0][feat1][feat2]  instance-2
            //  [feat0][feat1][feat2]] instance-3

            // label_Total
            // [label of instance-0, label of instance-1, label of instance-2, label of instance-3] = 
            // [1,1,2,2]

            int train_num = training_Total.Count;
            int dim = training_Total[0].Count;

            // Construct an array of training data.
            double[,] trainSet = List2array(training_Total);

            // Construct an array of test data.
            double[,] tstSet = List2array(Test_data);

            // norm evaluate
            int test_length = Test_data.Count;
            double[] distance = new double[train_num];
            double distance_temp = 0;
            List<double[]> sort_distance = new List<double[]>();
            double C_vote = new double(); // 最後投票結果

            for (int i_trial = 0; i_trial < test_length; i_trial++)
            {
                for (int i = 0; i < train_num; i++)
                {
                    distance_temp = norm(Row(tstSet, i_trial), Row(trainSet, i));
                    distance[i] = distance_temp;
                }

                sort_distance = sort(distance); // 排序過後的值 與 index

                double[] label = new double[K_value]; // k 取 3，只找前三個最近的資料點判斷
                for (int m = 0; m < K_value; m++)
                {
                    double vote_temp;
                    vote_temp = Convert.ToDouble(label_Total[Convert.ToInt32(sort_distance[1][m])-1]);
                    //Console.WriteLine("label_Total index:" + Convert.ToInt32(sort_distance[1][m]));
                    label[m] = vote_temp;
                }

                int freq1 = label.Count(n => (n == 1));
                int freq0 = label.Count(n => (n == 2));
                if (freq1 > freq0) C_vote = 1;
                else C_vote = 2;
            }

            return C_vote;
        }


        /// <summary>
        /// Maximum and index
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] maxnum(double[] data)
        {
            double maxvalue = new double();
            double index = new double();
            double[] result = new double[2];
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Equals(data.Max()))
                {
                    maxvalue = data[i];
                    index = i;
                }
            }
            result[0] = index;
            result[1] = maxvalue;
            return result;
        }

        /// <summary>
        /// Sorting
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        public static List<double[]> sort(double[] input)
        {
            var temp = input;
            int num = temp.GetLength(0);
            List<double[]> output = new List<double[]>();
            double[] temp_ind = new double[num]; // 原本位置順序
            double[] ind = new double[num];

            for (int i = 0; i < num; i++)
            {
                temp_ind[i] = i + 1;
            }

            for (int i = 0; i <= temp.Length; i++)
            {
                // 第二個迴圈，跑 Length-1 的次數
                for (int j = 0; j < temp.Length - 1; j++)
                {
                    // 如果前一格 大於 後一個值，則位置互換。
                    if (temp[j] > temp[j + 1])
                    {
                        double data_temp = temp[j];
                        temp[j] = temp[j + 1];
                        temp[j + 1] = data_temp;

                        double temp1 = temp_ind[j];
                        temp_ind[j] = temp_ind[j + 1];
                        temp_ind[j + 1] = temp1;
                    }
                }
            }
            output.Add(temp);
            output.Add(temp_ind);
            return output;
        }

        public static List<double[]> sortDescending(double[] input)
        {
            var temp = input;
            int num = temp.GetLength(0);
            List<double[]> output = new List<double[]>();
            double[] temp_ind = new double[num]; // 原本位置順序
            double[] ind = new double[num];

            for (int i = 0; i < num; i++)
            {
                temp_ind[i] = i + 1;
            }

            for (int i = 0; i <= temp.Length; i++)
            {
                // 第二個迴圈，跑 Length-1 的次數
                for (int j = 0; j < temp.Length - 1; j++)
                {
                    // 如果前一格 大於 後一個值，則位置互換。
                    if (temp[j] > temp[j + 1])
                    {
                        double data_temp = temp[j];
                        temp[j] = temp[j + 1];
                        temp[j + 1] = data_temp;

                        double temp1 = temp_ind[j];
                        temp_ind[j] = temp_ind[j + 1];
                        temp_ind[j + 1] = temp1;
                    }
                }
            }
            var tempRev = temp;
            var temp_indRev = temp_ind;
            for (int i = 0; i <= temp.Length; i++)
            {
                tempRev[temp.Length - 1 - i] = temp[i];
                temp_indRev[temp.Length - 1 - i] = temp_ind[i];
            }
            output.Add(tempRev);
            output.Add(temp_indRev);
            return output;
        }
        /// <summary>
        /// Euclidean distance evaluate
        /// </summary>
        /// <param name="test"></param>
        /// <param name="train"></param>
        /// <returns></returns>
        public static double norm(double[,] test, double[,] train)
        {
            int num = test.GetLength(1);
            double output = 0;
            for (int j = 0; j < num; j++)
            {
                double temp = test[0, j] - train[0, j];
                output += Math.Pow(temp, 2);
            }
            output = Math.Pow(output, 0.5);
            return output;
        }
        public static double norm(double[] test, double[,] train)
        {
            int num = train.GetLength(1);
            double output = 0;
            for (int j = 0; j < num; j++)
            {
                double temp = test[j] - train[0, j];
                output += Math.Pow(temp, 2);
            }
            output = Math.Pow(output, 0.5);
            return output;
        }
        public static double norm(List<double> test, List<double> train)
        {
            int num = test.Count;
            double output = 0;
            for (int j = 0; j < num; j++)
            {
                double temp = test[j] - train[j];
                output += Math.Pow(temp, 2);
            }
            output = Math.Pow(output, 0.5);
            return output;
        }

        /// <summary>
        /// 取出矩陣某列 (y : 取出第幾列出來)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[,] Row(double[,] x, int y)
        {
            double[,] output;
            int num_COL = x.GetLength(1);
            output = new double[1, num_COL];
            //y -= 1;
            for (int i = 0; i < num_COL; i++)
            {
                output[0, i] = x[y, i];
            }
            return output;
        }

        /// <summary>
        /// average
        /// </summary>
        /// <param name="x"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static double mean(double[] x)
        {
            double meanValue = new double();
            foreach (double v in x) meanValue += v;
            return meanValue/x.Length;
        }
        public static double[] mean(double[,] x, int dir)
        {
            double[] meanValue;
            meanValue = new double[x.GetLength(dir)];
            string flag;
            int nRow = x.GetLength(0);
            int nCol = x.GetLength(1);
            switch (dir)
            {
                case 1:
                    for (int col = 0; col < nCol; col++)
                    {
                        for (int row = 0; row < nRow; row++) meanValue[col] += x[row, col];
                        meanValue[col] = meanValue[col] / nRow;
                    }
                    break;
                case 0:
                    for (int row = 0; row < nRow; row++)
                    {
                        for (int col = 0; col < nCol; col++) meanValue[col] += x[row, col];
                        meanValue[row] = meanValue[row] / nCol;
                    }
                    break;
            }
            return meanValue;
        }
        public static double[] mean(double[][] x, int dir)
        {
            double[] meanValue;
            meanValue = new double[x.GetLength(dir)];
            string flag;
            int nRow = x.GetLength(0);
            int nCol = x[0].Length;
            switch (dir)
            {
                case 0:
                    for (int col = 0; col < nCol; col++)
                    {
                        for (int row = 0; row < nRow; row++) meanValue[col] += x[row][col];
                        meanValue[col] = meanValue[col] / nRow;
                    }
                    break;
                case 1:
                    for (int row = 0; row < nRow; row++)
                    {
                        for (int col = 0; col < nCol; col++) meanValue[row] += x[row][col];
                        meanValue[row] = meanValue[row] / nCol;
                    }
                    break;
            }
            return meanValue;
        }

        /// <summary>
        /// Band power(BP) evaluate
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bandstart"></param>
        /// <param name="bandend"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        /// 
        public static double bandpower(double[] data, int bandstart, int bandend, int time)
        {
            int point = bandstart * time; // 算 PSD 時，頻帶的起始值
            double bandcount = (bandend + 1 - bandstart) * time;
            double[,] Result_BP = new double[1, 1];
            //alglib.complex[] fftdata;
            alglib.fftr1d(data, out var fftdata);
            double Result = 0;
            for (int i = 0; i < bandcount; i++)
            {
                double temp1 = Math.Pow(fftdata[point + i].x, 2) + Math.Pow(fftdata[point + i].y, 2);
                Result += temp1;
            }
            //Result_BP[0, 0] = Result;
            return Result;
        }
        public static double bandpower(ref double[] data, int bandstart, int bandend, int time)
        {
            int point = bandstart * time; // 算 PSD 時，頻帶的起始值
            double bandcount = (bandend + 1 - bandstart) * time;
            double[,] Result_BP = new double[1, 1];
            //alglib.complex[] fftdata;
            alglib.fftr1d(data, out var fftdata);
            double Result = 0;
            for (int i = 0; i < bandcount; i++)
            {
                double temp1 = Math.Pow(fftdata[point + i].x, 2) + Math.Pow(fftdata[point + i].y, 2);
                Result += temp1;
            }
            //Result_BP[0, 0] = Result;
            return Result;
        }
        public static double bandpower(List<double> dataList, int bandstart, int bandend, int time)
        {
            double[] data = new double[dataList.Count];
            foreach (var item in dataList.Select((value, i) => new { i, value })) data[item.i] = item.value;
            int point = bandstart * time; // 算 PSD 時，頻帶的起始值
            double bandcount = (bandend + 1 - bandstart) * time;
            double[,] Result_BP = new double[1, 1];
            alglib.complex[] fftdata;
            alglib.fftr1d(data, out fftdata);
            double Result = 0;
            for (int i = 0; i < bandcount; i++)
            {
                double temp1 = Math.Pow(fftdata[point + i].x, 2) + Math.Pow(fftdata[point + i].y, 2);
                Result += temp1;
            }
            //Result_BP[0, 0] = Result;
            return Result;
        }

        /// <summary>
        /// band power(BP) evaluate(online)
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bandstart"></param>
        /// <param name="bandend"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double[,] bandpower_Online(double[] data, int bandstart, int bandend, int time)
        {
            int point = bandstart * time; // 算 PSD 時，頻帶的起始值
            double bandcount = (bandend - bandstart) * time;
            double[,] Result_BP = new double[1, 1];
            alglib.complex[] fftdata;
            alglib.fftr1d(data, out fftdata);
            double Result = 0;
            for (int i = 0; i < bandcount; i++)
            {
                double temp1 = Math.Pow(fftdata[point + i].x, 2) + Math.Pow(fftdata[point + i].y, 2);
                Result += temp1;
            }
            Result_BP[0, 0] = Result;
            return Result_BP;
        }

        /// <summary>
        /// jagged to 2D array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T[,] To2D<T>(T[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The given jagged array is not rectangular.");
            }
        }
        public static T[][] ToJagged<T>(T[,] twoDimensionalArray)
        {
            int rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
            int rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
            int numberOfRows = rowsLastIndex + 1;

            int columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
            int columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
            int numberOfColumns = columnsLastIndex + 1;

            T[][] jaggedArray = new T[numberOfRows][];
            for (int i = rowsFirstIndex; i <= rowsLastIndex; i++)
            {
                jaggedArray[i] = new T[numberOfColumns];

                for (int j = columnsFirstIndex; j <= columnsLastIndex; j++)
                {
                    jaggedArray[i][j] = twoDimensionalArray[i, j];
                }
            }
            return jaggedArray;
        }

        /// <summary>
        /// PSD evaluate
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bandstart"></param>
        /// <param name="bandend"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double[,] PSD(double[] data, int bandstart, int bandend, int time)
        {
            int point = bandstart * time; // 算 PSD 時，頻帶的起始值
            double bandcount = (bandend - bandstart) * time;
            double[,] Result_BP = new double[1, 1];
            alglib.complex[] fftdata;
            alglib.fftr1d(data, out fftdata);
            double Result = 0;
            for (int i = 0; i < bandcount; i++)
            {
                double temp1 = Math.Pow(fftdata[point + i].x, 2) + Math.Pow(fftdata[point + i].y, 2);
                Result += temp1;
            }
            Result_BP[0, 0] = Result;
            return Result_BP;
        }

        /// <summary>
        /// List to 2D array
        /// </summary>
        /// <param name="list2D"></param>
        /// <returns></returns>
        public static double[,] List2array(List<List<double>> list2D)
        {
            double[,] array2D = new double[list2D.Count, list2D[0].Count];
            for(int i = 0; i < list2D.Count; i++)
            {
                for(int j = 0; j < list2D[0].Count; j++)
                {
                    array2D[i, j] = list2D[i][j];
                }
            }
            return array2D;
        }

        /// <summary>
        /// 計算費雪值，無排序 Fisher's score evaluation, no sorting.
        /// </summary>
        /// <param name="type0"></param>
        /// <param name="type1"></param>
        /// <returns></returns>
        public static double[] fishers(List<List<double>> type0, List<List<double>> type1)
        {
            // type1 is "class 1" //type2 is "class 2"
            // [[feat0][feat1][feat2]  instance-0
            //  [feat0][feat1][feat2]  instance-1
            //  [feat0][feat1][feat2]  instance-2
            //  [feat0][feat1][feat2]] instance-3
            int dim = type1[0].Count; //Obatain the dimension of feature sets.
            double[] fisherScore = new double[dim];
            // List<double[]> fisherScore = new List<double[]>();
            double[][] arrType0 = ToJagged(List2array(type0));
            double[][] arrType1 = ToJagged(List2array(type1));
            double[] meanFeat_0 = mean(To2D(arrType0), 1);
            double[] meanFeat_1 = mean(To2D(arrType1), 1);
            var nType0 = arrType0.GetLength(0);
            var nType1 = arrType1.GetLength(0);
            var nTotal = nType0 + nType1;
            double[][] sampleCov0 = new double[dim][];
            for(int i = 0; i < dim; i++) sampleCov0[i] = new double[dim];
            double[][] sampleCov1 = new double[dim][];
            for (int i = 0; i < dim; i++) sampleCov1[i] = new double[dim];
            double[][] arrType0_MinusMean = plusMatrix(arrType0, NegtMatrix(Duplicate(meanFeat_0, nType0)));
            double[][] arrType0_MinusMean_Transpose = Transpose(plusMatrix(arrType0, NegtMatrix(Duplicate(meanFeat_0, nType0))));
            sampleCov0 = MatrixMult(arrType0_MinusMean_Transpose, arrType0_MinusMean);

            sampleCov0 = MatrixMult(sampleCov0, 1/Convert.ToDouble(nType0));
            //sampleCov0 = MatrixMult(MatrixMult(Transpose(plusMatrix(arrType0, NegtMatrix(Duplicate(meanFeat_0, nType0)))),
            //    plusMatrix(arrType0, NegtMatrix(Duplicate(meanFeat_0, nType0)))), 1/ nType0);
            sampleCov1 = MatrixMult(MatrixMult(Transpose(plusMatrix(arrType1, NegtMatrix(Duplicate(meanFeat_1, nType1)))),
                plusMatrix(arrType1, NegtMatrix(Duplicate(meanFeat_1, nType1)))), 1 / Convert.ToDouble(nType1));
            double[][] scatWithIn = new double[dim][];
            scatWithIn = plusMatrix(MatrixMult(sampleCov0, Convert.ToDouble(nType0)/ Convert.ToDouble(nTotal)), MatrixMult(sampleCov1, Convert.ToDouble(nType1)/ Convert.ToDouble(nTotal))); // scatter within class

            double[] meanFeat = new double[dim];
            for (int i = 0; i < dim; i++) meanFeat[i] = (meanFeat_0[i]*nType0 + meanFeat_1[i]*nType1)/nTotal;
            double[][] scatBetween = new double[dim][];
            Algorithm alg = new Algorithm();
            scatBetween = alg.Scatter(meanFeat_0, meanFeat_1, nType0, nType1, meanFeat); // scatter between class

            // scatBetween和scatWithIn相除後，取對角線的值
            for (int i = 0; i < dim; i++) { fisherScore[i] = scatBetween[i][i] / scatWithIn[i][i]; }
            return fisherScore;
        }

        /// <summary>
        /// Between-class scatter
        /// </summary>
        /// <param name="array0"></param>
        /// <param name="array1"></param>
        /// <param name="n0"></param>
        /// <param name="n1"></param>
        /// <param name="arrayTotal"></param>
        /// <returns></returns>
        private double[][] Scatter(double[] array0, double[] array1, int n0, int n1, double[] arrayTotal)
        {
            var dim = array0.Length;
            double[][] scatBetween = new double[dim][];
            for (int i = 0; i < dim; i++) scatBetween[i] = new double[dim];
            double[][] matrix0 = Duplicate(array0, 1);
            double[][] matrix1 = Duplicate(array1, 1);
            double[][] matrixTotal = Duplicate(arrayTotal, 1);
            var nTotal = Convert.ToDouble(n0 + n1);
            scatBetween = MatrixMult(MatrixMult(Transpose(plusMatrix(matrix0, NegtMatrix(matrixTotal))),
                plusMatrix(matrix0, NegtMatrix(matrixTotal))), Convert.ToDouble(n0)/nTotal);
            scatBetween = plusMatrix(scatBetween, MatrixMult(MatrixMult(Transpose(plusMatrix(matrix1, NegtMatrix(matrixTotal))),
                plusMatrix(matrix1, NegtMatrix(matrixTotal))), Convert.ToDouble(n1)/nTotal));
            return scatBetween;
        }

        /// <summary>
        /// 判斷一個二維陣列是否為矩陣
        /// </summary>
        /// <param name="matrix">二維陣列</param>
        /// <returns>true:是矩陣 false:不是矩陣</returns>
        public static bool isMatrix(double[][] matrix)
        {
            //空矩陣是矩陣
            if (matrix.Length < 1) return true;
            //不同行列數如果不相等，則不是矩陣
            int count = matrix[0].Length;
            for (int i = 1; i < matrix.Length; i++)
            {
                if (matrix[i].Length != count)
                {
                    return false;
                }
            }
            //各行列數相等，則是矩陣
            return true;
        }

        /// <summary>
        /// 計算一個矩陣的行數和列數
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <returns>陣列：行數、列數</returns>
        public static int[] MatrixColRow(double[][] matrix)
        {
            //接收到的引數不是矩陣則報異常
            if (!isMatrix(matrix))
            {
                throw new Exception("接收到的引數不是矩陣");
            }
            //空矩陣行數列數都為0
            if (!isMatrix(matrix) || matrix.Length == 0)
            {
                return new int[2] { 0, 0 };
            }
            return new int[2] { matrix.Length, matrix[0].Length };
        }

        // ----------- 矩陣計算 ---------------------
        /// <summary>
        /// 矩陣相加
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static double[][] plusMatrix(double[][] matrix1, double[][] matrix2)
        {
            //合法性檢查
            if (!isMatrix(matrix1))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix1.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix1.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix1[i].Length];
            }
            //矩陣取負：各元素取相反數
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = matrix1[i][j] + matrix2[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// 矩陣取負
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <returns>負矩陣</returns>
        public static double[][] NegtMatrix(double[][] matrix)
        {
            //合法性檢查
            if (!isMatrix(matrix))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //矩陣取負：各元素取相反數
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = -matrix[i][j];
                }
            }
            return result;
        }

        /// <summary>
        /// 矩陣數乘
        /// </summary>
        /// <param name="matrix">矩陣</param>
        /// <param name="num">常數</param>
        /// <returns>積</returns>
        public static double[][] MatrixMult(double[][] matrix, double num)
        {
            //合法性檢查
            if (!isMatrix(matrix))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //矩陣數乘：用常數依次乘以矩陣各元素
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = matrix[i][j] * num;
                }
            }
            return result;
        }
        public static double[][] MatrixMult(double[][] matrix, int num)
        {
            //合法性檢查
            if (!isMatrix(matrix))
            {
                throw new Exception("傳入的引數並不是一個矩陣");
            }
            //引數為空矩陣則返回空矩陣
            if (matrix.Length == 0)
            {
                return new double[][] { };
            }
            //生成一個與matrix同型的空矩陣
            double[][] result = new double[matrix.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[matrix[i].Length];
            }
            //矩陣數乘：用常數依次乘以矩陣各元素
            for (int i = 0; i < result.Length; i++)
            {
                for (int j = 0; j < result[0].Length; j++)
                {
                    result[i][j] = matrix[i][j] * num;
                }
            }
            return result;
        }


        /// <summary>
        /// 矩陣乘法
        /// </summary>
        /// <param name="matrix1">矩陣1</param>
        /// <param name="matrix2">矩陣2</param>
        /// <returns>積</returns>
        public static double[][] MatrixMult(double[][] matrix1, double[][] matrix2)
        {
            //合法性檢查
            if (MatrixColRow(matrix1)[1] != MatrixColRow(matrix2)[0])
            {
                throw new Exception("matrix1 的列數與 matrix2 的行數不想等");
            }
            //矩陣中沒有元素的情況
            if (matrix1.Length == 0 || matrix2.Length == 0)
            {
                return new double[][] { };
            }
            //matrix1是m*n矩陣，matrix2是n*p矩陣，則result是m*p矩陣
            int m = matrix1.Length, n = matrix2.Length, p = matrix2[0].Length;
            double[][] result = new double[m][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new double[p];
            }
            //矩陣乘法：c[i,j]=Sigma(k=1→n,a[i,k]*b[k,j])
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < p; j++)
                {
                    //對乘加法則
                    for (int k = 0; k < n; k++)
                    {
                        result[i][j] += (matrix1[i][k] * matrix2[k][j]);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 矩陣轉置 Transpose the matrix
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[,] Transpose(double[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            double[,] result = new double[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }

            return result;
        }
        public static double[][] Transpose(double[][] matrix)
        {
            int nRow = matrix.GetLength(0);
            int nCol = matrix[0].Length;

            double[][] result = new double[nCol][];
            for (int tRow = 0; tRow < nCol; tRow++) result[tRow] = new double[nRow];

            for(int tRow=0; tRow < nCol; tRow++)
            {
                for(int tCol=0; tCol < nRow; tCol++)
                {
                    result[tRow][tCol] = matrix[tCol][tRow];
                }
            }

            //for (int i = 0; i < w; i++)
            //{
            //    for (int j = 0; j < h; j++)
            //    {
            //        result[j][i] = matrix[i][j];
            //    }
            //}

            return result;
        }

        /// <summary>
        /// 複製行向量為指定列數的array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static double[][] Duplicate(double[] array, int layer)
        {
            double[][] result = new double[layer][];
            for (int i = 0; i < layer; i++) result[i] = array;
            return result;
        }
    }
}

