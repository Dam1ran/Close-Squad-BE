using CS.Core.Entities.Auth;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Auth;
public class IdentificationConfiguration : EntityConfiguration<Identification> {
  public override string TableName => "Identifications";
  protected override string? Schema => "cs.auth";

  public override void OnConfigure(EntityTypeBuilder<Identification> builder) {

    builder
      .Property(csu => csu.Role)
      .HasMaxLength(20)
      .IsRequired();

    builder
      .HasOne(csu => csu.IdentificationPassword)
      .WithOne()
      .HasForeignKey<IdentificationPassword>("identificationId")
      .IsRequired();

    builder
      .HasOne(csu => csu.IdentificationRefreshToken)
      .WithOne()
      .HasForeignKey<IdentificationRefreshToken>("identificationId")
      .OnDelete(DeleteBehavior.Cascade);

  }

}
