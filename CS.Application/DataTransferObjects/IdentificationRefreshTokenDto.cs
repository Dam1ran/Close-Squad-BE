using CS.Core.Entities.Auth;

namespace CS.Application.DataTransferObjects;
public class IdentificationRefreshTokenDto {
  public string Token { get; set; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; set; }

  public static IdentificationRefreshTokenDto FromIrt(IdentificationRefreshToken irt) =>
    new IdentificationRefreshTokenDto { Token = irt.RefreshToken, ExpiresAt = irt.ExpiresAt };

}
