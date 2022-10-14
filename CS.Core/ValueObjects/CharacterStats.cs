using CS.Core.Entities.Abstractions;

namespace CS.Core.ValueObjects;
public class CharacterStats {

  public Stat Hp { get; set; } = new();
  public Stat Mp { get; set; } = new();
  public Stat Speed { get; set; } = new();

  public void RegenerationTick() {
    Hp.RegenerationTick();
    Mp.RegenerationTick();
  }

  public bool HasHp() => Hp.Current > 0;

}
