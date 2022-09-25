using CS.Core.Entities.Abstractions;

namespace CS.Core.Entities;
public class Character : Entity {
  #nullable disable
  protected Character() { }
  #nullable restore
  public Character(Quadrant quadrant) {
    Quadrant = quadrant;
  }

  public byte Level { get; set; }

  public long QuadrantId { get; set; }
  public Quadrant Quadrant { get; set; }

}
