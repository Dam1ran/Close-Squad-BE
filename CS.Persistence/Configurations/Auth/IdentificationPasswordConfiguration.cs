using CS.Core.Entities.Auth;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Auth;
public class IdentificationPasswordConfiguration : EntityConfiguration<IdentificationPassword> {
  public override string TableName => "IdentificationPasswords";
  protected override string? Schema => "cs.auth";

  public override void OnConfigure(EntityTypeBuilder<IdentificationPassword> builder) {}

}
