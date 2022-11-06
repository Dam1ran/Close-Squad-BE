using CS.Core.Entities;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;

namespace CS.Core.ValueObjects;
public class Stats {
  public IReadOnlyList<Tuple<StatType, Stat>> StatsList { get; private set; } = null!;
  public void Init() {
    StatsList = new List<Tuple<StatType, Stat>>() {
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


  public void ApplyEffects(IEnumerable<Effect> effects) {
    foreach (var effect in effects) {
      ApplyEffect(effect);
    }
  }
  public void ApplyEffect(Effect effect) =>
    StatsList.Single(s => s.Item1 == effect.StatType).Item2.Set(effect.StatOperation, effect.Value);

  public void ClearEffect(Effect effect) =>
    StatsList.Single(s => s.Item1 == effect.StatType).Item2.Set(effect.StatOperation, -effect.Value);

  public void UpdateStat(StatType statType, StatOperation statOperation, double value) =>
    StatsList.Single(s => s.Item1 == statType).Item2.Set(statOperation, value);

  public bool CanUseToggleSkill(Skill skill) =>
    (!skill.HpConsumePerTick.HasValue || Hp.Current > skill.HpConsumePerTick + 1) &&
    (!skill.MpConsumePerTick.HasValue || Mp.Current > skill.MpConsumePerTick + 1);

  public void ConsumeTick(Skill skill) {
    if (skill.HpConsumePerTick.HasValue) {
      UpdateStat(StatType.Hp, StatOperation.AddCurrentAmount, -skill.HpConsumePerTick.Value);
    }
    if (skill.MpConsumePerTick.HasValue) {
      UpdateStat(StatType.Mp, StatOperation.AddCurrentAmount, -skill.MpConsumePerTick.Value);
    }
  }
  public bool CanBeConsumed(Skill skill) =>
    (!skill.HpConsume.HasValue || Hp.Current > skill.HpConsume + 1) &&
    (!skill.MpConsume.HasValue || Mp.Current > skill.MpConsume + 1);

  public void Consume(Skill skill) {
    if (skill.HpConsume.HasValue) {
      UpdateStat(StatType.Hp, StatOperation.AddCurrentAmount, -skill.HpConsume.Value);
    }
    if (skill.MpConsume.HasValue) {
      UpdateStat(StatType.Mp, StatOperation.AddCurrentAmount, -skill.MpConsume.Value);
    }
  }

}
