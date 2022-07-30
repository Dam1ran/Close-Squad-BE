using CS.Core.Entities.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Abstractions;
public abstract class EntityConfiguration<TEntity> : ConfigurationBase<TEntity> where TEntity : Entity {
  public abstract override string TableName { get; }

  public virtual bool UseHiLo { get; } = false;

  protected string HiLoSequenceName => $"{TableName}HiLoSequence";

  public override void Configure(EntityTypeBuilder<TEntity> builder) {
    builder.HasKey(q => q.Id);

    if (UseHiLo) {
      builder.Property(q => q.Id).UseHiLo(HiLoSequenceName).IsRequired();
    }

    base.Configure(builder);
  }
}