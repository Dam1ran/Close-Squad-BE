using CS.Core.Entities.Abstractions;
using CS.Core.Exceptions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities.Auth;
public class CsUser : Entity {
  #nullable disable
  protected CsUser() { }
  #nullable restore

  public CsUser (Nickname nickname, Email email, Password password) {
    Nickname = nickname ?? throw new DomainValidationException(nameof(nickname));
    Verification = new(email);
    Identification = new(password);
    Player = new Player(nickname);
  }

  public Nickname Nickname { get; private set; }

  public Verification Verification { get; private set; }
  public Identification Identification { get; private set; }

  public Player Player { get; private set; }

}
