using CS.Core.Entities.Abstractions;
using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : Entity { // NOT DONE

  public Character(string name, User user) {
    Name = new Name(name);
    User = user;
  }
  #nullable disable
  protected Character() {}
  #nullable restore

  public Name Name { get; set; }

  public User User { get; set; }
}
