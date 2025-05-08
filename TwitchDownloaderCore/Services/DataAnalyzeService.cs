using System;
using System.Collections.Generic;
using System.Linq;
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
        public static List<DateTime> FindHotCommentsTimeline(ChatRoot root, TimeSpan? interval = null)
        {
            if (interval == null)
                interval = DEFAULT_TIME_INTERVAL;

            List<DateTime> timelineList = new();

            // 分组并计数
            var timelineData = root.comments
                .GroupBy(d => new DateTime(
                    d.created_at.Year,
                    d.created_at.Month,
                    d.created_at.Day,
                    d.created_at.Hour,
                    d.created_at.Minute / 5 * 5, // 将分钟向下取整到最接近的5分钟倍数
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

            var selectedTimeline = orderbyCount
                 .Where(s => s.Count > highThr)
                 .ToList();

            var timeLineOffsets = selectedTimeline
                .Select(s => TimeSpan.FromSeconds(s.BeginOffset))
                .ToList();

            foreach (var item in selectedTimeline)
            {
                timelineList.Add(item.TimeSlot);
            }

            return timelineList;
        }
    }
}
