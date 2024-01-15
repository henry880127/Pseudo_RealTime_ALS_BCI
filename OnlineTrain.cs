using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pseudo_RealTime_ALS_BCI
{
    internal class OnlineTrain
    {
        #region sockets
        public Socket serverSocket, clientSocket;
        #endregion

        public List<double[,]> Data_group1 { get; set; } = new List<double[,]>();
        public List<double[,]> Data_group2 { get; set; } = new List<double[,]>();

        public List<List<double>> FeatureVec_group1 { get; set; } = new List<List<double>>();
        public List<List<double>> FeatureVec_group2 { get; set; } = new List<List<double>>();

        public List<List<double>> OptFeatureVec_group1 { get; set; } = new List<List<double>>();
        public List<List<double>> OptFeatureVec_group2 { get; set; } = new List<List<double>>();

        public List<int[]> OptConfig { get; set; } = new List<int[]>();

        public int TestSecond { get; set; } = 3;
        public int Sampling { get; set; } = 500;

        public List<int[]> FeatConfig { get; set; } = new List<int[]>();

        public double CR_opt { get; set; } = new double();

        public List<int[]> FeatureSelection(int K_value)
        {
            //List<int[]> optConfig = new List<int[]>();

            int[] label_group1 = new int[Data_group1.Count];
            for (int i = 0; i < label_group1.Length; i++) label_group1[i] = 1;

            int[] label_group2 = new int[Data_group2.Count];
            for (int i = 0; i < label_group2.Length; i++) label_group2[i] = 2;

            FeatureVec_group1 = ExtractAllFeat(Data_group1);
            FeatureVec_group2 = ExtractAllFeat(Data_group2);
            double[] fisherScore = Algorithm.fishers(FeatureVec_group1, FeatureVec_group2);
            List<double[]> fisherScoreIndex = Algorithm.sort(fisherScore);
            int FSnum = fisherScore.Count();

            var featureVec_Total = FeatureVec_group1.Concat(FeatureVec_group2).ToList();
            var label_Total = label_group1.Concat(label_group2).ToArray();

            int numInstance = Data_group1.Count + Data_group2.Count;
            int k = numInstance; // 自訂k值，LOO:k = instance總數

            double[] CR = new double[numInstance - 1];  // 最高特徵維度只到Instance總數-1
            for (int dim = 1; dim < numInstance; dim++)
            {
                double[] label_clf = new double[k];
                for(int kth = 0; kth < k; kth += numInstance/k)
                {
                    var traningFeatureVec = featureVec_Total.Where((_, i) => i != kth).ToList();
                    var chosenInd = fisherScoreIndex[1].Skip(FSnum - dim).ToList();
                    for (var inst = 0; inst < traningFeatureVec.Count; inst++)
                    {
                        traningFeatureVec[inst] = traningFeatureVec[inst].Where((_,i)=>chosenInd.Contains(i)).ToList();
                    }
                    var trainingLabel = label_Total.Where((_, i) => i != kth).ToArray();
                    var testFeatureVec = featureVec_Total.Take(kth+1).Skip(kth).ToList();
                    for (var inst = 0; inst < testFeatureVec.Count; inst++)
                    {
                        testFeatureVec[inst] = testFeatureVec[inst].Where((_, i) => chosenInd.Contains(i)).ToList();
                    }
                    var testLabel = label_Total.Take(kth+1).Skip(kth).ToArray();

                    label_clf[kth] = Algorithm.KNN_Online(traningFeatureVec, trainingLabel, testFeatureVec, K_value);
                    //if (testLabel[0] == Convert.ToInt32(label_clf)) CR[dim] += 1 / Convert.ToDouble(k);
                }
                for (int i = 0; i < k; i++)
                {
                    if (label_Total[i] == Convert.ToInt32(label_clf[i])) CR[dim-1] += 1 / Convert.ToDouble(k);
                }
            }

            var CR_sort = Algorithm.sort(CR);
            CR_opt = CR_sort[0][numInstance - 2];
            var opt_Dim = CR_sort[1][numInstance/2 - 1];
            //var bestIndex = fisherScoreIndex[1].Take(Convert.ToInt32(CR_sort[1][numInstance - 2]));
            var bestIndex = fisherScoreIndex[1].Skip(FSnum - Convert.ToInt32(opt_Dim)).ToArray();
            for (int trial = 0; trial < FeatureVec_group1.Count; trial++)
            {
                OptFeatureVec_group1.Add(new List<double>());
                OptFeatureVec_group2.Add(new List<double>());
            }
            for(int i = 0; i < bestIndex.Length; i++)
            {
                var temp_Ind = bestIndex[i];
                OptConfig.Add(FeatConfig[Convert.ToInt32(temp_Ind - 1)]);
                for (int trial = 0; trial < FeatureVec_group1.Count; trial++)
                {
                    OptFeatureVec_group1[trial].Add(FeatureVec_group1[trial][Convert.ToInt32(temp_Ind - 1)]);
                    OptFeatureVec_group2[trial].Add(FeatureVec_group2[trial][Convert.ToInt32(temp_Ind - 1)]);
                }
            }
            //foreach (var i in bestIndex)
            //{
            //    OptConfig.Add(FeatConfig[Convert.ToInt32(i - 1)]);
            //    for (int trial = 0; trial < FeatureVec_group1.Count; trial++)
            //    {
            //        OptFeatureVec_group1[trial].Add(FeatureVec_group1[trial][Convert.ToInt32(i - 1)]);
            //        OptFeatureVec_group2[trial].Add(FeatureVec_group2[trial][Convert.ToInt32(i - 1)]);
            //    }
            //} 
            //optConfig = featConfig.Take(Convert.ToInt32(bestIndex)).ToList();
            return OptConfig;
        }

        public void BuildFeatConfig(int numChannel, int numFeat)
        {
            for(int i = 0; i < numChannel; i++)
            {
                for(var j = 0; j < numFeat; j++)
                {
                    int[] chFeature = new int[] { i, j };
                    FeatConfig.Add(chFeature);
                }
            }
        }

        public List<List<double>> ExtractAllFeat(List<double[,]>  data)
        {
            FeatConfig.Clear();
            for (int i = 0; i < 6; i++)
            {
                for (var j = 0; j < 6; j++)
                {
                    int[] chFeature = new int[] { i+1, j+1 };
                    FeatConfig.Add(chFeature);
                }
            }
            //BuildFeatConfig(6, 6); //建置6通道、6種特徵之" 通道/特徵index 集合"
            CustomArray<double> customArray = new CustomArray<double>();
            List<List<double>> featureVec = new List<List<double>>();
            
            double[] tempEEG = new double[data[0].GetLength(1)];
            for(int trial = 0; trial < data.Count; trial++)
            {
                featureVec.Add(new List<double>());
                foreach (var chFeature in FeatConfig)
                {
                    //List<double> tempEEG = new List<double>();
                    switch (chFeature[0])
                    {
                        case 1:  // ch-1
                            tempEEG = customArray.GetRow(data[trial], 0);
                            break;
                        case 2:  // ch-2
                            tempEEG = customArray.GetRow(data[trial], 1);
                            break;
                        case 3:  // ch-3
                            tempEEG = customArray.GetRow(data[trial], 2);
                            break;
                        case 4:  // ch-1, ch-2 bipolar
                            for (int i = 0; i < data[0].GetLength(0); i++)
                                tempEEG[i] = data[trial][0, i] - data[trial][1, i];
                            break;
                        case 5:  // ch-1, ch-3 bipolar
                            for (int i = 0; i < data[0].GetLength(0); i++)
                                tempEEG[i] = data[trial][0, i] - data[trial][2, i];
                            break;
                        case 6:  // ch-2, ch-3 bipolar
                            for (int i = 0; i < data[0].GetLength(0); i++)
                                tempEEG[i] = data[trial][1, i] - data[trial][2, i];
                            break;
                    }
                    switch (chFeature[1])
                    {
                        case 1:
                            featureVec[trial].Add(Math.Log(Algorithm.KFD_Function(tempEEG)));
                            break;
                        case 2:
                            featureVec[trial].Add(Math.Log(Algorithm.bandpower(tempEEG, 4, 8, TestSecond)));
                            break;
                        case 3:
                            featureVec[trial].Add(Math.Log(Algorithm.bandpower(tempEEG, 8, 13, TestSecond)));
                            break;
                        case 4:
                            featureVec[trial].Add(Math.Log(Algorithm.bandpower(tempEEG, 13, 20, TestSecond)));
                            break;
                        case 5:
                            featureVec[trial].Add(Math.Log(Algorithm.bandpower(tempEEG, 20, 30, TestSecond)));
                            break;
                        case 6:
                            featureVec[trial].Add(Math.Log(Algorithm.bandpower(tempEEG, 30, 45, TestSecond)));
                            break;
                    }
                }
            }
            

            return featureVec;
        }

        public List<List<double>> LogNormalization(List<List<double>> matrix)
        {
            List<List<double>> result = new List<List<double>>();
            result = matrix;
            for(int row = 0; row < matrix.Count; row++)
            {
                for (int col = 0; col < matrix[row].Count; col++)
                {
                    result[row][col] = Math.Log(matrix[row][col]);
                }
            }
            return result;
        }

        
    }
}
