using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Player: Entity {
  #nullable disable
  protected Player() { }
  #nullable restore
  public Player(Nickname nickname) {
    Nickname = nickname;
    Characters = new List<Character>();
  }
  public string ClanName { get; set; } = "";
  public string ClanIcon { get; set; } = "";
  public Nickname Nickname { get; private set; }


  [NotMapped]
  public string? ConnectionId { get; set; }
  [NotMapped]
  public uint? QuadrantIndex { get; set; }

  [NotMapped]
  public DateTimeOffset? LogoutAt { get; set; }

  [NotMapped]
  public List<string> QuadrantsUrl { get; set; } = new();

  public IList<Character> Characters { get; set; }

}
