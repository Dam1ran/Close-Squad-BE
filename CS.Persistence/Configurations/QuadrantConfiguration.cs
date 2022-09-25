using CS.Core.Entities;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations;
public class QuadrantConfiguration : EntityConfiguration<Quadrant> {
  public override string TableName => "Quadrants";

  public override void OnConfigure(EntityTypeBuilder<Quadrant> builder) {
    builder
      .HasMany(q => q.Players)
      .WithOne(c => c.Quadrant)
      .HasForeignKey("QuadrantId")
      .OnDelete(DeleteBehavior.NoAction);

    builder
      .HasMany(q => q.Characters)
      .WithOne(c => c.Quadrant)
      .HasForeignKey("QuadrantId")
      .OnDelete(DeleteBehavior.NoAction);

  }

}
