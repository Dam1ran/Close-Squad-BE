using CS.Core.Entities.Abstractions;

namespace CS.Core.ValueObjects;
public class Stats {
  public Stat Hp { get; set; } = new();
  public Stat Mp { get; set; } = new();

  public Stat PhysicalAttack { get; set; } = new();
  public Stat PhysicalAttackSpeed { get; set; } = new();
  public Stat PhysicalDefense { get; set; } = new();
  public Stat AttackRange { get; set; } = new();

  public Stat Speed { get; set; } = new();

  private void Tick() {
    Hp.Tick();
    Mp.Tick();
  }

  public void Tick(bool canRegenerate) {
    if (canRegenerate) {
      Tick();
    }
  }

  public bool HasHp() => Hp.Current > 0;

}
