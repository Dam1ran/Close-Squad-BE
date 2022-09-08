using CS.Core.Entities.Abstractions;
using CS.Core.Exceptions;
using CS.Core.Extensions;

namespace CS.Core.Entities.Auth;
public class IdentificationRefreshToken : Entity {
  #nullable disable
  protected IdentificationRefreshToken() { }
  #nullable restore

  public IdentificationRefreshToken(string refreshToken, DateTimeOffset expiresAt) {
    if (string.IsNullOrWhiteSpace(refreshToken) || !expiresAt.IsInFuture()) {
      throw new DomainValidationException("Wrong arguments for Identification RefreshToken creation");
    }
    RefreshToken = refreshToken;
    ExpiresAt = expiresAt;
  }

  public string RefreshToken { get; private set; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; private set; } = DateTimeOffset.UtcNow;

  public void Clear() {
    RefreshToken = string.Empty;
    ExpiresAt = DateTimeOffset.MinValue;
  }

}
