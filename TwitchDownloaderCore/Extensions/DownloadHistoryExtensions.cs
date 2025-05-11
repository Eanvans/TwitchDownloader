using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using TwitchDownloaderCore.Models;

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
    }

    public class TwitchDownloaderDB : DbContext
    {
        public TwitchDownloaderDB() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=twitchDownloader.db");
        }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.ApplyConfiguration(new DownloadHistoryConfiguration());

        }
    }

    public class DownloadHistoryConfiguration : IEntityTypeConfiguration<DownloadHistory>
    {
        public void Configure(EntityTypeBuilder<DownloadHistory> builder)
        {
            builder.ToTable(nameof(DownloadHistory))
                .HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .IsRequired();
        }
    }
}
