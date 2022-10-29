using CS.Core.Enums;

namespace CS.Core.Entities;
public class Skill {

  public int SkillKeyId { get; set; }
  public int SkillId { get; set; }
  public int Level { get; set; }

  public SkillActivationType ActivationType { get; set; }
  public SkillTargetType TargetType { get; set; }
  public SkillKindType Kind { get; set; }
  public SkillEffectType EffectType { get; set; }

  public List<EffectStat>? EffectStats { get; set; }
  public long? SkillEffectId { get; set; }

  public double? MpConsume { get; set; }
  public double? HpConsume { get; set; }
  public double? MpConsumePerTick { get; set; }
  public double? HpConsumePerTick { get; set; }
  public double? EffectSeconds { get; set; }
  public double? CoolDownMs { get; set; }
  public double? CastRange { get; set; }
  public double? EffectRange { get; set; }

}

public class EffectStat {
  public StatType Type { get; set; }
  public StatOperation Operation { get; set; }
  public double Value { get; set; }
}
