using CS.Core.Entities.Abstractions;
using CS.Core.Enums;

namespace CS.Core.ValueObjects;
public class Stats {
  public IReadOnlyList<Tuple<StatType, Stat>> Collection { get; private set; } = null!;
  public void Init() {
    Collection = new List<Tuple<StatType, Stat>>() {
      new(StatType.Hp, Hp),
      new(StatType.Mp, Mp),
      new(StatType.PhysicalAttack, PhysicalAttack),
      new(StatType.PhysicalAttackSpeed, PhysicalAttackSpeed),
      new(StatType.PhysicalDefense, PhysicalDefense),
      new(StatType.AttackRange, AttackRange),
      new(StatType.Speed, Speed),
      new(StatType.CastingSpeed, CastingSpeed),
    }.AsReadOnly();
  }
  public Stat Hp { get; set; } = new();
  public Stat Mp { get; set; } = new();

  public Stat PhysicalAttack { get; set; } = new();
  public Stat PhysicalAttackSpeed { get; set; } = new();
  public Stat PhysicalDefense { get; set; } = new();
  public Stat AttackRange { get; set; } = new();

  public Stat Speed { get; set; } = new();

  public Stat CastingSpeed { get; set; } = new();

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
