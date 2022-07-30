using Microsoft.AspNetCore.Identity;

namespace CS.Core.Entities.Auth;
public class User : IdentityUser<long> {
  public IEnumerable<Character> Characters { get; set; } = new List<Character>();
}
