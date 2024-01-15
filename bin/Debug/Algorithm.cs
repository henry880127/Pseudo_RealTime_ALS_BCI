using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ALS
{
    class Algorithm
    {
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




        //最大值和索引
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
        // 排序
        public static List<double[]> sort(double[] temp)
        {
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

        // 歐式距離
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

        //取出矩陣某列 (y : 取出第幾列出來)
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
        


    }
}
