using CS.Core.Entities.Abstractions;

namespace CS.Core.Entities;
public class Quadrant : Entity {
  #nullable disable
  protected Quadrant() { }
  #nullable restore
  public Quadrant(ushort xIndex, ushort yIndex  /*other*/) {
    XIndex = xIndex;
    YIndex = yIndex;
  }

  public ushort XIndex { get; private set; }
  public ushort YIndex { get; private set; }

}
