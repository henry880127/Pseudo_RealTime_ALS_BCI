using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pseudo_RealTime_ALS_BCI
{
    internal class SyncBCI
    {
        public SyncBCI() { }
        public SyncBCI(List<int[]> optConfig) { OptConfig = optConfig; }

        public SyncBCI(List<int[]> optConfig, int testSecond) { OptConfig = optConfig; TestSecond = testSecond; }

        public double[,] CurrentData { get; set; }

        public List<double[,]> CurrentDataList { get; set; }

        public List<List<double>> FeatureVec { get; set; } = new List<List<double>>();

        public List<double> CurrentFeatureVec { get; set; } = new List<double>();

        public int TestSecond { get; set; } = 4; // Default as 4 seconds.

        public List<int[]> OptConfig { get; set; }

        /// <summary>
        /// Extract features of "CurrentDataList" based on optConfig ("optimal configuration")
        /// </summary>
        /// <returns></returns>
        public List<List<double>> ExtractFeat_CurrentDataList()
        {
            if (CurrentFeatureVec != null) CurrentFeatureVec.Clear();
            if (FeatureVec != null) FeatureVec.Clear();
            CustomArray<double> customArray = new CustomArray<double>();
            foreach(var data in CurrentDataList)
            {
                if (CurrentFeatureVec != null) CurrentFeatureVec.Clear();
                double[] tempEEG = new double[data.GetLength(1)];
                foreach (var chFeature in OptConfig)
                {
                    //List<double> tempEEG = new List<double>();
                    switch (chFeature[0])
                    {
                        case 1:
                            tempEEG = customArray.GetRow(data, 0);
                            break;
                        case 2:
                            tempEEG = customArray.GetRow(data, 1);
                            break;
                        case 3:
                            tempEEG = customArray.GetRow(data, 2);
                            break;
                        case 4:
                            for (int i = 0; i < data.GetLength(1); i++)
                                tempEEG[i] = data[0, i] - data[1, i];
                            break;
                        case 5:
                            for (int i = 0; i < data.GetLength(1); i++)
                                tempEEG[i] = data[0, i] - data[2, i];
                            break;
                        case 6:
                            for (int i = 0; i < data.GetLength(1); i++)
                                tempEEG[i] = data[1, i] - data[2, i];
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
            }
            return FeatureVec;
        }
        private List<double> ExtractFeat_CurrentDataList(double[,] data)
        {
            if (CurrentFeatureVec != null) CurrentFeatureVec.Clear();
            if (FeatureVec != null) FeatureVec.Clear();
            CustomArray<double> customArray = new CustomArray<double>();

            double[] tempEEG = new double[data.GetLength(1)];
            foreach (var chFeature in OptConfig)
            {
                //List<double> tempEEG = new List<double>();
                switch (chFeature[0])
                {
                    case 1:
                        tempEEG = customArray.GetRow(data, 0);
                        break;
                    case 2:
                        tempEEG = customArray.GetRow(data, 1);
                        break;
                    case 3:
                        tempEEG = customArray.GetRow(data, 2);
                        break;
                    case 4:
                        for (int i = 0; i < data.GetLength(1); i++)
                            tempEEG[i] = data[0, i] - data[1, i];
                        break;
                    case 5:
                        for (int i = 0; i < data.GetLength(1); i++)
                            tempEEG[i] = data[0, i] - data[2, i];
                        break;
                    case 6:
                        for (int i = 0; i < data.GetLength(1); i++)
                            tempEEG[i] = data[1, i] - data[2, i];
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
        public List<List<double>> ExtractFeat_currentData()
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
            return FeatureVec;
        }

        public double[] Votes { get; set; } = { 0, 0, 0 };  
        public double OnlineClassify(List<List<double>> training_data1, List<List<double>> training_data2, int K_value)
        {
            // ExtractFeat_currentData(CurrentData);
            int i = 0;
            foreach (double[,] data in CurrentDataList)
            {
                Votes[i] = Algorithm.KNN_Online(training_data1, training_data2, ExtractFeat_CurrentDataList(data), K_value);
                i++;
            }
            return OnlineVoting();
        }

        public double TempResult { get; set; }

        public int nEpoch { get; set; } = 3;
        public int OnlineVoting()
        {
            int freq1 = Votes.Count(n => (n == 1));
            int freq2 = Votes.Count(n => (n == 2));
          
            if (freq2 > (Votes.Length / 2)) return 2;
            else if (freq1 > (Votes.Length /2)) return 1;
            else return 0;
        }
        public int OnlineVoting(int[] attentionVotes)
        {
            int freq1 = attentionVotes.Count(n => (n == 1));
            int freq0 = attentionVotes.Count(n => (n == 2));
            if (attentionVotes.Length >= nEpoch)
                if (freq1 > (attentionVotes.Length / 2)) return 1;
                else return 2;
            else return 0;
        }
        public string Voting(int[] attentionVotes)
        {
            int freq1 = attentionVotes.Count(n => (n == 1));
            int freq0 = attentionVotes.Count(n => (n == 2));
            if (attentionVotes.Length >= nEpoch)
                if (freq1 > (attentionVotes.Length/2)) return "Attention";
                else return "Rest";
            else return "null";
        }

        /// <summary>
        /// voting,return "attention" or "rest"; if there's not enough epochs(default as 10) return "null"
        /// </summary>
        /// <returns></returns>
        public string Voting()
        {
            int freq1 = Votes.Count(n => (n == 1));
            int freq0 = Votes.Count(n => (n == 2));
            if (Votes.Length >= nEpoch)
                if (freq1 > (Votes.Length / 2)) return "rest";
                else return "attention";
            else return "Null";
        }


    }
}
