using CS.Core.Entities.Abstractions;

namespace CS.Core.Entities;
public class Quadrant : Entity {
  #nullable disable
  protected Quadrant() { }
  #nullable restore
  public Quadrant(ushort xIndex, ushort yIndex  /*other*/) {
    Characters = new List<Character>();
    Players = new List<Player>();
    XIndex = xIndex;
    YIndex = yIndex;
  }

  public IEnumerable<Player> Players { get; private set; }
  public IEnumerable<Character> Characters { get; private set; }
  public ushort XIndex { get; private set; }
  public ushort YIndex { get; private set; }

}
