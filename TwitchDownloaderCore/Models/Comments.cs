using System;
using System.Diagnostics;
using TwitchDownloaderCore.TwitchObjects;

namespace TwitchDownloaderCore.Models
{
    [DebuggerDisplay("{commenter} {message}")]
    public class Comment
    {
        public string _id { get; set; }
        public DateTime created_at { get; set; }
        public string channel_id { get; set; }
        public string content_type { get; set; }
        public string content_id { get; set; }
        public double content_offset_seconds { get; set; }
        public Commenter commenter { get; set; }
        public Message message { get; set; }

        public Comment Clone()
        {
            return new Comment()
            {
                _id = _id,
                created_at = created_at,
                channel_id = channel_id,
                content_type = content_type,
                content_id = content_id,
                content_offset_seconds = content_offset_seconds,
                commenter = commenter.Clone(),
                message = message.Clone()
            };
        }
    }
}
