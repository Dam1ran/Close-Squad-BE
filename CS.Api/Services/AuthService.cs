using System.Security.Cryptography;

namespace CS.Api.Services;
public class AuthService { // NOT DONE
  public RefreshToken Generate() {
    return new RefreshToken {
      Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
      Expires = DateTime.Now.AddDays(7),
      Created = DateTime.Now
    };
  }
}

public class RefreshToken {
  public string Token { get; set; } = string.Empty;
  public DateTime Created { get; set; } = DateTime.Now;
  public DateTime Expires { get; set; }
}
