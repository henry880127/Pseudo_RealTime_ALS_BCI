using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
//using static ALS.Algorithm;

namespace Pseudo_RealTime_ALS_BCI
{
    public class AsyncDetection
    {
        public AsyncDetection() { }
        public AsyncDetection(int threshold) { Threshold = threshold; }
        public List<double[,]> Data { get; set; } = new List<double[,]>();
        public double[,] CurrentData { get; set; }
        public List<List<double>> FeatureVec { get; set; } = new List<List<double>>();
        public List<double> CurrentFeatureVec { get; set; } = new List<double>();
        public int TestSecond { get; set; } = 5; // Default as 5 seconds.
        public List<int[]> OptConfig { get; set; }

        /// <summary>
        /// Feature extraction based on "optimal configuration".
        /// First elements in double[] means channel.(ch1,2,3,1-2,1-3,2-3);
        /// Second ones imply feature type (KFD,theta_BP,alpha_BP,betaLow_BP,betaHigh_BP,gamma_BP)
        /// </summary>
        public List<List<double>> ExtractFeat(List<int[]> optConfig)
        {
            CustomArray<double> customArray = new CustomArray<double>();
            FeatureVec.Clear();
            double[] tempEEG = new double[Data[0].GetLength(0)];
            foreach (var chFeature in optConfig)
            {
                //List<double> tempEEG = new List<double>();
                switch (chFeature[0])
                {
                    case 1:
                        tempEEG = customArray.GetRow(Data[0], 0);
                        break;
                    case 2:
                        tempEEG = customArray.GetRow(Data[0], 1);
                        break;
                    case 3:
                        tempEEG = customArray.GetRow(Data[0], 2);
                        break;
                    case 4:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][0,i] - Data[0][1,i];
                        break;
                    case 5:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][0,i] - Data[0][2,i];
                        break;
                    case 6:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][1,i] - Data[0][2,i];
                        break;
                }
                switch (chFeature[1])
                {
                    case 1:
                        FeatureVec[0].Add(Math.Log(Algorithm.KFD_Function(tempEEG)));
                        break;
                    case 2:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 4, 8, TestSecond)));
                        break;
                    case 3:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 8, 13, TestSecond)));
                        break;
                    case 4:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 13, 20, TestSecond)));
                        break;
                    case 5:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 20, 30, TestSecond)));
                        break;
                    case 6:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 30, 45, TestSecond)));
                        break;
                }
            }
            
            return FeatureVec;
        }
        public List<List<double>> ExtractFeat()
        {
            CustomArray<double> customArray = new CustomArray<double>();
            if (FeatureVec.Any()) FeatureVec.Clear();

            double[] tempEEG = new double[Data[0].GetLength(0)];
            foreach (var chFeature in OptConfig)
            {
                //List<double> tempEEG = new List<double>();
                switch (chFeature[0])
                {
                    case 1:
                        tempEEG = customArray.GetRow(Data[0], 0);
                        break;
                    case 2:
                        tempEEG = customArray.GetRow(Data[0], 1);
                        break;
                    case 3:
                        tempEEG = customArray.GetRow(Data[0], 2);
                        break;
                    case 4:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][0, i] - Data[0][1, i];
                        break;
                    case 5:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][0, i] - Data[0][2, i];
                        break;
                    case 6:
                        for (int i = 0; i < Data[0].GetLength(0); i++)
                            tempEEG[i] = Data[0][1, i] - Data[0][2, i];
                        break;
                }
                switch (chFeature[1])
                {
                    case 1:
                        FeatureVec[0].Add(Math.Log(Algorithm.KFD_Function(tempEEG)));
                        break;
                    case 2:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 4, 8, TestSecond)));
                        break;
                    case 3:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 8, 13, TestSecond)));
                        break;
                    case 4:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 13, 20, TestSecond)));
                        break;
                    case 5:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 20, 30, TestSecond)));
                        break;
                    case 6:
                        FeatureVec[0].Add(Math.Log(Algorithm.bandpower(tempEEG, 30, 45, TestSecond)));
                        break;
                }
            }

            return FeatureVec;
        }

        public List<double> ExtractFeat_currentData()
        {
            if (CurrentFeatureVec != null) CurrentFeatureVec.Clear();

            CustomArray<double> customArray = new CustomArray<double>();

            double[] tempEEG = new double[CurrentData.GetLength(1)];
            foreach (var chFeature in OptConfig)
            {
                //List<double> tempEEG = new List<double>();
                switch (chFeature[0])
                {
                    case 1:
                        tempEEG = customArray.GetRow(CurrentData, 0);
                        break;
                    case 2:
                        tempEEG = customArray.GetRow(CurrentData, 1);
                        break;
                    case 3:
                        tempEEG = customArray.GetRow(CurrentData, 2);
                        break;
                    case 4:
                        for (int i = 0; i < CurrentData.GetLength(1); i++)
                            tempEEG[i] = CurrentData[0, i] - CurrentData[1, i];
                        break;
                    case 5:
                        for (int i = 0; i < CurrentData.GetLength(1); i++)
                            tempEEG[i] = CurrentData[0, i] - CurrentData[2, i];
                        break;
                    case 6:
                        for (int i = 0; i < CurrentData.GetLength(1); i++)
                            tempEEG[i] = CurrentData[1, i] - CurrentData[2, i];
                        break;
                }
                switch (chFeature[1])
                {
                    case 1:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.KFD_Function(tempEEG)));
                        break;
                    case 2:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 4, 8, TestSecond)));
                        break;
                    case 3:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 8, 13, TestSecond)));
                        break;
                    case 4:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 13, 20, TestSecond)));
                        break;
                    case 5:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 20, 30, TestSecond)));
                        break;
                    case 6:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 30, 45, TestSecond)));
                        break;
                }
            }
            FeatureVec.Add(CurrentFeatureVec);
            return CurrentFeatureVec;
        }
        public List<double> ExtractFeat_currentData(double[,] currentData)
        {
            //if (CurrentFeatureVec != null) CurrentFeatureVec.Clear();
            CurrentFeatureVec.Clear();
            CustomArray<double> customArray = new CustomArray<double>();

            double[] tempEEG = new double[currentData.GetLength(1)];
            foreach (var chFeature in OptConfig)
            {
                //List<double> tempEEG = new List<double>();
                switch (chFeature[0])
                {
                    case 1:
                        tempEEG = customArray.GetRow(currentData, 0);
                        break;
                    case 2:
                        tempEEG = customArray.GetRow(currentData, 1);
                        break;
                    case 3:
                        tempEEG = customArray.GetRow(currentData, 2);
                        break;
                    case 4:
                        for (int i = 0; i < currentData.GetLength(1); i++)
                            tempEEG[i] = currentData[0, i] - currentData[1, i];
                        break;
                    case 5:
                        for (int i = 0; i < currentData.GetLength(1); i++)
                            tempEEG[i] = currentData[0, i] - currentData[2, i];
                        break;
                    case 6:
                        for (int i = 0; i < currentData.GetLength(1); i++)
                            tempEEG[i] = currentData[1, i] - currentData[2, i];
                        break;
                }
                switch (chFeature[1])
                {
                    case 1:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.KFD_Function(tempEEG)));
                        break;
                    case 2:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 4, 8, TestSecond)));
                        break;
                    case 3:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 8, 13, TestSecond)));
                        break;
                    case 4:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 13, 20, TestSecond)));
                        break;
                    case 5:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 20, 30, TestSecond)));
                        break;
                    case 6:
                        CurrentFeatureVec.Add(Math.Log(Algorithm.bandpower(tempEEG, 30, 45, TestSecond)));
                        break;
                }
            }
            FeatureVec.Add(CurrentFeatureVec);
            return CurrentFeatureVec;
        }
        /// <summary>
        /// 根據此class中當前的data進行分類，並回傳類別
        /// </summary>
        /// <param name="training_data1"></param>
        /// <param name="training_data2"></param>
        /// <param name="K_value"></param>
        /// <returns></returns>
        /// 
        public double classify(List<List<double>> training_data1, List<List<double>> training_data2, int K_value)
        {
            TempResult = Algorithm.KNN_Online(training_data1, training_data2, CurrentFeatureVec, K_value);
            return TempResult;
        }
        public double classify(List<List<double>> training_data1, List<List<double>> training_data2,List<double> currentFeatureVec, int K_value)
        {
            TempResult = Algorithm.KNN_Online(training_data1, training_data2, currentFeatureVec, K_value);
            return TempResult;
        }
        public double TempResult { get; set; }

        public int[] AttentionVotes { get; set; } = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Update candidate sequence. 更新投票序列
        /// </summary>
        /// <param name="newVote"></param>
        /// <returns></returns>
        public int[] Forward(int newVote)
        {
            if (AttentionVotes.Length >= nEpoch)
            {
                for (int i = 0; i < nEpoch-1; i++)
                {
                    AttentionVotes[i] = AttentionVotes[i + 1];
                }
                AttentionVotes[nEpoch-1] = newVote;
            }
            return AttentionVotes;
        }
        /// <summary>
        /// Update candidate sequence, based on the result of the last classify( ). 更新投票序列(以上一次進行classify的結果)
        /// </summary>
        /// <param name="newVote"></param>
        /// <returns></returns>
        public int[] Forward()
        {
            if (AttentionVotes.Length >= nEpoch)
            {
                for (int i = 0; i < nEpoch-1; i++)
                {
                    AttentionVotes[i] = AttentionVotes[i + 1];
                }
                AttentionVotes[nEpoch-1] = Convert.ToInt32(TempResult);
            }  
            return AttentionVotes;
        }

        /// <summary>
        /// 
        /// </summary>
        private void dataForward()
        {

            if (Data.Count() >= nEpoch)
            {
                for (int i = 0; i < nEpoch-1; i++)
                {
                    Data[i] = Data[i + 1];
                }
                Data[nEpoch-1] = CurrentData;
            }
            else
            {
                Data.Add(CurrentData);
            }
        }

        public int nEpoch { get; set; } = 10; // Default as 10 epochs, in order to take a vote.
        public int Threshold { get; set; } = 5; // Default as 5 epochs.

        public int OnlineVoting(int[] attentionVotes)
        {
            int freq1 = attentionVotes.Count(n => (n == 1));
            int freq0 = attentionVotes.Count(n => (n == 2));
            if (attentionVotes.Length >= nEpoch)
                if (freq1 > Threshold) return 1;
                else return 2;
            else return 0;
        }

        /// <summary>
        /// voting,return "attention" or "rest"; if there's not enough epochs(default as 10) return "null"
        /// </summary>
        /// <param name="attentionVotes"></param>
        /// <returns></returns>
        public string Voting(int[] attentionVotes)
        {
            int freq1 = attentionVotes.Count(n => (n == 1));
            int freq2 = attentionVotes.Count(n => (n == 2));
            if (freq1 > Threshold) return "rest";
            else if (freq2 > Threshold) return "attention";
            else return "Null";
        }
        /// <summary>
        /// voting,return "attention" or "rest"; if there's not enough epochs(default as 10) return "null"
        /// </summary>
        /// <returns></returns>
        public string Voting()
        {
            int freq1 = AttentionVotes.Count(n => (n == 1));
            int freq2 = AttentionVotes.Count(n => (n == 2));
            
            if (freq1 > Threshold) return "rest";
            else if (freq2 > Threshold) return "attention";
            else return "Null";
        }

    }
    
}
