using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace iMusicSync.Data.Context.Entities.Base
{
    public abstract class EntityBase
    {
    }

    public abstract class EntityBaseTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityBase
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
        }
    }
}
