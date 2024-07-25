using iMusicSync.Data.Context.Entities.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iMusicSync.Data.Context.Entities
{
    public class MusicEntry : EntityWithIdBase
    {
        public string Album { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public long Duration { get; set; }
        public string Path { get; set; }
        public string Hash { get; set; }
    }

    public class MusicEntrynConfiguration : EntityWithIdBaseTypeConfiguration<MusicEntry>
    {
        public override void Configure(EntityTypeBuilder<MusicEntry> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.Title)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.Artist)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.Duration)
                .IsRequired();

            builder.Property(e => e.Path)
                .HasMaxLength(512);

            builder.Property(e => e.Hash)
                .HasMaxLength(32);
        }
    }
}
