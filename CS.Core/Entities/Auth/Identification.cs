using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities.Auth;
public class Identification : Entity {
  #nullable disable
  protected Identification() { }
  #nullable restore
  public Identification(Password password) {
    SetIdentificationPassword(password);
  }
  public string Role { get; private set; } = UserRole.USR.ToString();
  public IdentificationPassword IdentificationPassword { get; private set; } = null!;
  public IdentificationRefreshToken IdentificationRefreshToken { get; private set; } = null!;

  public void SetRole(UserRole userRole) => Role = userRole.ToString();
  public void SetRefreshToken(IdentificationRefreshToken identificationRefreshToken) => IdentificationRefreshToken = identificationRefreshToken;
  public void SetIdentificationPassword(Password password) => IdentificationPassword = new (password);

}
