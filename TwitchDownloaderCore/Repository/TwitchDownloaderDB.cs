using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitchDownloaderCore.Models;

namespace TwitchDownloaderCore.Repository
{
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
