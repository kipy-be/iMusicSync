using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace iMusicSync.Data.Context.Entities.Base
{
    public abstract class EntityWithIdBase : EntityBase
    {
        public long Id { get; set; }
    }

    public abstract class EntityWithIdBaseTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityWithIdBase
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}
