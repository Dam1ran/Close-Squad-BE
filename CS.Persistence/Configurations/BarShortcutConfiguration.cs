using CS.Core.Entities;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class BarShortcutConfiguration : EntityConfiguration<BarShortcut> {

  public override string TableName => "BarShortcuts";

  public override void OnConfigure(EntityTypeBuilder<BarShortcut> builder) {
    builder.Ignore(bs => bs.IsActive);
  }
}
