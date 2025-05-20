using System;
using System.Collections.Generic;
using System.Linq;
using TwitchDownloaderCore.Models;
using TwitchDownloaderCore.TwitchObjects;

namespace TwitchDownloaderCore.Services
{
    public static class DataAnalyzeService
    {
        private static TimeSpan DEFAULT_TIME_INTERVAL = TimeSpan.FromMinutes(5);

        /// <summary>
        /// use IQR method find the Q3 and Q1
        /// IQR = Q3 - Q1 
        /// highlight time is time interval about Q3 + 1.5*IQR 
        /// </summary>
        public static List<VodCommentData> FindHotCommentsTimelineIQR(ChatRoot root,
            TimeSpan? interval = null)
        {
            if (interval == null)
                interval = DEFAULT_TIME_INTERVAL;

            // 分组并计数
            var timelineData = root.comments
                .GroupBy(d => new DateTime(
                    d.created_at.Year,
                    d.created_at.Month,
                    d.created_at.Day,
                    d.created_at.Hour,
                    d.created_at.Minute, // 将分钟向下取整到最接近的5分钟倍数
                    0))
                .Select(g => new
                {
                    TimeSlot = g.Key,
                    Count = g.Count(),
                    BeginOffset = g.FirstOrDefault().content_offset_seconds
                })
                .OrderBy(x => x.TimeSlot);

            var orderbyTimeline = timelineData.ToList();

            var orderbyCount = timelineData
                .OrderBy(s => s.Count)
                .ToList();

            var q1 = orderbyCount[(int)(orderbyCount.Count * 0.25)].Count;
            var q3 = orderbyCount[(int)(orderbyCount.Count * 0.75)].Count;
            var iqr = q3 - q1;
            var highThr = q3 + (int)(1.5 * iqr);

            VodCommentStats stat = new VodCommentStats();
            foreach (var item in orderbyTimeline)
            {
                stat.AddData(new VodCommentData()
                {
                    TimeInterval = item.TimeSlot.ToShortDateString(),
                    CommentsCount = item.Count,
                    OffsetSeconds = item.BeginOffset,
                });
            }

            var selectedTimeline = orderbyCount
                 .Where(s => s.Count > highThr)
                 .ToList();

            var selected3SigmaTimeline = orderbyCount
                .Where(s => s.Count > 3 * stat.Sigma)
                .ToList();


            var timeLineOffsets = selectedTimeline
               .Select(s => TimeSpan.FromSeconds(s.BeginOffset))
               .ToList();

            List<VodCommentData> rst = new();
            foreach (var item in selectedTimeline)
            {
                rst.Add(new VodCommentData()
                {
                    TimeInterval = item.TimeSlot.ToShortDateString(),
                    CommentsCount = item.Count,
                    OffsetSeconds = item.BeginOffset,
                });
            }

            rst = rst.OrderByDescending(s => s.CommentsCount).ToList();
            return rst;
        }

        /// <summary>
        /// 使用滑动滤波的方式过滤峰值
        /// </summary>
        /// <param name="root"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static List<VodCommentData> FindHotCommentsIntervalSlidingFilter(ChatRoot root, TimeSpan? interval = null)
        {
            if (interval == null)
                interval = DEFAULT_TIME_INTERVAL;

            int commLen = root.comments.Count;
            var t = root.comments.Select(s => s.content_offset_seconds).ToList();
            int dt = 5; // 5s

            // 计算 T 的最大值
            double tStart = t.FirstOrDefault();
            double tEnd = t.LastOrDefault();
            double maxTime = tEnd - tStart + dt;

            // 构建 T 并初始化 count 数组
            int tLength = (int)(maxTime / dt) + 1; // 包含最后一个完整区间
            double[] T = new double[tLength];
            double[] count = new double[tLength];

            for (int i = 0; i < tLength; i++)
            {
                T[i] = i * dt;
            }

            // 分配计数
            for (int i = 0; i < t.Count; i++)
            {
                double timeOffset = t[i] - tStart;
                int k = (int)Math.Floor(timeOffset / dt);
                if (k >= 0 && k < tLength)
                {
                    count[k]++;
                }
                else
                {
                    Console.WriteLine($"Warning: time value {t[i]} is out of T range.");
                }
            }

            // 第一步：计算窗口长度
            var tWindowLength = (int)(10 * 60) / dt;
            // 第二步：调用 MeanFilter（需要前面定义的函数）
            double[] filteredCount = AlgoService.MeanFilter(count, tWindowLength + 1);
            // 第三步：对结果进行缩放
            double scale = tWindowLength + 1;
            double[] count1 = new double[filteredCount.Length];
            for (int i = 0; i < filteredCount.Length; i++)
            {
                count1[i] = filteredCount[i] * scale;
            }

            // 第四步：截取 T 中间部分：T1 = T(1 + WL/2 : end - WL/2)
            int wl = tWindowLength;
            int startIdx = (int)(wl / 2.0); // MATLAB 是从 1 开始索引，所以这里是 1 + wl/2 - 1
            int endIdx = T.Length - (int)(wl / 2.0) - 1;

            int resultLength = endIdx - startIdx + 1;
            double[] T1 = new double[resultLength];
            Array.Copy(T, startIdx, T1, 0, resultLength);

            DetectPeaks(count1, tWindowLength, out List<int> peakIndex, out List<double> peak, out double meanVal);

            // peakTrue 是峰值
            FilterTruePeaks(peakIndex, peak, tWindowLength, out List<int> peakIndexTrue, out List<double> peakTrue);

            // 提取 peakT：从 T1 中取出对应索引的时间值 加上第一个值的second offset 才是准确的时间值
            List<double> peakT = new List<double>();
            foreach (int index in peakIndexTrue)
            {
                if (index >= 0 && index < T1.Length)
                {
                    peakT.Add(T1[index]);
                }
                else
                {
                    Console.WriteLine($"Warning: Index {index} out of range for T1.");
                }
            }

            // 查找时间段
            foreach (double p in peakTrue)
            {

            }

            // 综合结果
            var timeLineOffsets = peakT
               .Select(s => TimeSpan.FromSeconds((int)s))
               .ToList();

            List<VodCommentData> rst = new();
            foreach (var item in timeLineOffsets)
            {
                rst.Add(new VodCommentData()
                {
                    TimeInterval = item.ToString(@"hh\:mm\:ss"),
                    OffsetInterval = item.ToString(@"hh\:mm\:ss")
                });
            }

            return rst;
        }

        public static void DetectPeaks(double[] count1, int tWindowLength, out List<int> peakIndex, out List<double> peak,
            out double meanVal)
        {
            // 计算阈值：1.3 * mean(count1)
            double sum = 0;
            foreach (var val in count1)
            {
                sum += val;
            }
            meanVal = sum / count1.Length;
            double thr = 1.3 * meanVal;  //调整这个值来探测更多的峰值

            peakIndex = new List<int>();
            peak = new List<double>();

            for (int i = 0; i <= count1.Length - tWindowLength - 1; i++)
            {
                // 提取窗口数据
                double[] tmpData = new double[tWindowLength + 1];
                Array.Copy(count1, i, tmpData, 0, tWindowLength + 1);

                // 找最大值
                double maxVal = double.MinValue;
                int maxIdxInWindow = -1;
                for (int j = 0; j < tmpData.Length; j++)
                {
                    if (tmpData[j] > maxVal)
                    {
                        maxVal = tmpData[j];
                        maxIdxInWindow = j;
                    }
                }

                // 跳过未超过阈值的情况
                if (maxVal < thr)
                    continue;

                // 排除窗口边缘的极值点
                if (maxIdxInWindow == 0 || maxIdxInWindow == tmpData.Length - 1)
                    continue;

                // 全局索引
                int globalIndex = i + maxIdxInWindow;

                // 判断是否已经记录了这个峰值
                if (peakIndex.Count == 0)
                {
                    peakIndex.Add(globalIndex);
                    peak.Add(maxVal);
                }
                else if (globalIndex == peakIndex[peakIndex.Count - 1])
                {
                    continue; // 避免重复添加
                }
                else
                {
                    peakIndex.Add(globalIndex);
                    peak.Add(maxVal);
                }
            }
        }

        public static void FilterTruePeaks(List<int> peakIndex, List<double> peak, int tWindowLength,
        out List<int> peakIndexTrue, out List<double> peakTrue)
        {
            peakIndexTrue = new List<int>();
            peakTrue = new List<double>();

            for (int i = 0; i < peak.Count; i++)
            {
                // 创建临时列表并删除第 i 个元素
                List<int> peakIndexTmp = new List<int>(peakIndex);
                List<double> peakTmp = new List<double>(peak);

                peakIndexTmp.RemoveAt(i);
                peakTmp.RemoveAt(i);

                int nowIndex = peakIndex[i];
                double nowPeak = peak[i];

                bool isTrue = true;

                for (int j = 0; j < peakTmp.Count; j++)
                {
                    int distance = Math.Abs(peakIndexTmp[j] - nowIndex);

                    if (distance > tWindowLength)
                        continue;

                    if (peakTmp[j] > nowPeak)
                    {
                        isTrue = false;
                        break;
                    }
                }

                if (isTrue)
                {
                    peakIndexTrue.Add(nowIndex);
                    peakTrue.Add(nowPeak);
                }
            }
        }
    }
}
