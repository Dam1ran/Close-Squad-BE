using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Abstractions;
public abstract class ConfigurationBase<T> : IEntityTypeConfiguration<T> where T : class {
  public abstract string TableName { get; }
  protected virtual string? Schema { get; set; } = null;

  public virtual void Configure(EntityTypeBuilder<T> builder) {
    builder.ToTable(TableName, Schema);
    OnConfigure(builder);
  }

  public abstract void OnConfigure(EntityTypeBuilder<T> builder);
}
