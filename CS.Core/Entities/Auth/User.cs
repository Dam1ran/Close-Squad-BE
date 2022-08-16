using CS.Core.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace CS.Core.Entities.Auth;
public class User : IdentityUser<long> {
  protected User() { }
  public User (string nickname, string email) {
    if (string.IsNullOrWhiteSpace(nickname) || string.IsNullOrWhiteSpace(email)) {
      throw new DomainValidationException("Wrong arguments for User creation");
    }
    UserName = nickname;
    Email = email;
    EmailConfirmed = false;
  }
  public bool CheckCaptcha { get; private set; } = false;
  public IEnumerable<Character> Characters { get; private set; } = new List<Character>(); // NOT DONE
  public void EnableCheckCaptcha() => CheckCaptcha = true;
  public void DisableCheckCaptcha() => CheckCaptcha = false;
}
