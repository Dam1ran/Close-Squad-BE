using CS.Core.Entities.Abstractions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Player: Entity {
  #nullable disable
  protected Player() { }
  #nullable restore
  public Player(Nickname nickname, Quadrant quadrant) {
    Nickname = nickname;
    Quadrant = quadrant;
    Characters = new List<Character>();
  }
  public string ClanName { get; set; } = "";
  public string ClanIcon { get; set; } = "";
  public Nickname Nickname { get; private set; }

  public Quadrant? Quadrant { get; set; }
  public IList<Character> Characters { get; set; }

}
