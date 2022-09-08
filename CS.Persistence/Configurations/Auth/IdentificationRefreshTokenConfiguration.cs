using CS.Core.Entities.Auth;
using CS.Persistence.Configurations.Abstractions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CS.Persistence.Configurations.Auth;
public class IdentificationRefreshTokenConfiguration : EntityConfiguration<IdentificationRefreshToken> {
  public override string TableName => "IdentificationRefreshTokens";
  protected override string? Schema => "cs.auth";

  public override void OnConfigure(EntityTypeBuilder<IdentificationRefreshToken> builder) {}

}
