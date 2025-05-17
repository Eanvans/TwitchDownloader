using System;

namespace TwitchDownloaderCore.Models
{
    public class VodCommentData
    {
        private double _offsetSeconds;

        public string TimeInterval { get; set; }
        public int CommentsCount { get; set; }
        public double OffsetSeconds
        {
            get => _offsetSeconds; set
            {
                _offsetSeconds = value;
                TimeSpan timeSpan = TimeSpan.FromSeconds(value);
                OffsetInterval = timeSpan.ToString(@"hh\:mm\:ss");
            }
        }
        public string OffsetInterval { get; set; }
    }
}
