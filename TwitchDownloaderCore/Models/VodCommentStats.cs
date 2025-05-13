using System;

namespace TwitchDownloaderCore.Models
{
    /// <summary>
    /// Calc the statistics
    /// </summary>
    public class VodCommentStats
    {
        private int min = 0;
        private int max = 0;
        private double average = 0;
        private double sum = 0;
        private double sumOfSquares = 0;
        private double sigma = 0;
        private int count = 0;

        public int Min { get => min; set => min = value; }
        public int Max { get => max; set => max = value; }
        public double Average { get => average; set => average = value; }
        public double Sum { get => sum; set => sum = value; }
        public double SumOfSquares { get => sumOfSquares; set => sumOfSquares = value; }
        public double Sigma { get => sigma; set => sigma = value; }
        public int Count { get => count; set => count = value; }

        public void AddData(VodCommentData data)
        {
            int value = data.CommentsCount;

            if (Count == 0)
            {
                Min = value;
                Max = value;
                Sum = value;
                Average = value;
                SumOfSquares = value * value;
                Sigma = 0;
                Count = 1;
                return;
            }

            if (value < Min) Min = value;
            if (value > Max) Max = value;

            Sum += value;
            Average = Sum / (double)(Count + 1);

            SumOfSquares += value * value;

            double variance = (SumOfSquares / (Count + 1)) - (Average * Average);
            Sigma = Math.Sqrt(variance);

            Count++;
        }
    }
}
