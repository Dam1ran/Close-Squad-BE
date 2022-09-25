using CS.Core.Entities.Abstractions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Player: Entity {
  #nullable disable
  protected Player() { }
  #nullable restore
  public Player(Nickname nickname) {
    Characters = new List<Character>();
    Nickname = nickname;
  }
  public string ClanName { get; set; } = "";
  public string ClanIcon { get; set; } = "";
  public Nickname Nickname { get; private set; }

  public long? QuadrantId { get; set; }
  public Quadrant? Quadrant { get; set; }
  public IEnumerable<Character> Characters { get; set; }

}