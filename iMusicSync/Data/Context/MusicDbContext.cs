using iMusicSync.Data.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace iMusicSync.Data.Context
{
    public class MusicDbContext : DbContext
    {
        public DbSet<MusicEntry> MusicEntries { get; set; }

        public string DbPath { get; }

        public MusicDbContext(string dbPath)
        {
            DbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
