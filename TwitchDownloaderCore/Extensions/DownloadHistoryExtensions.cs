using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using TwitchDownloaderCore.Models;
using TwitchDownloaderCore.Repository;

namespace TwitchDownloaderCore.Extensions
{
    public static class DownloadHistoryExtensions
    {
        /// <summary>
        /// save only
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        public static bool SaveDownloadHistory(string link)
        {
            TwitchDownloaderDB db = Appbase.ServiceProvider.GetService<TwitchDownloaderDB>();

            db.Set<DownloadHistory>()
                .Add(new DownloadHistory()
                {
                    Link = link
                });
            db.SaveChanges();

            return true;
        }

        public static List<DownloadHistory> GetAllDownloadHistories()
        {
            TwitchDownloaderDB db = Appbase.ServiceProvider.GetService<TwitchDownloaderDB>();

            return db.Set<DownloadHistory>()
                .ToList()
                .DistinctBy(s => s.Link)
                .ToList();
        }

        /// <summary>
        /// cache downloaded vod comments
        /// saved by time and void link
        /// </summary>
        public static void CacheVodComments()
        {

        }
    }
}
