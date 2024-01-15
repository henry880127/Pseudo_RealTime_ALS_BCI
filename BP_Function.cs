using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pseudo_RealTime_ALS_BCI
{
    class BP_Function
    {
        public static double bandpower(double[] data, int bandstart, int bandend, int time)
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
            //Result_BP[0, 0] = Result;
            return Result;
        }


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


        

    }
}
