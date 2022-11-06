using CS.Core.Enums;

namespace CS.Core.Entities;
public class Skill {

  public long SkillKeyId { get; set; }

  public TargetType TargetType { get; set; }


  public List<long> EffectorKeyIds { get; set; } = new();
  public IEnumerable<Effector> Effectors { get; set; } = new List<Effector>();

  public double? MpConsume { get; set; }
  public double? HpConsume { get; set; }
  // public ?? item consume, count { get; set; }

  public double? MpConsumePerTick { get; set; }
  public double? HpConsumePerTick { get; set; }

  public double? CoolDownMs { get; set; }
  public double? CastRange { get; set; }


  public SkillActivationType GetActivationType() {
    if (MpConsumePerTick.HasValue || HpConsumePerTick.HasValue) {
      return SkillActivationType.Toggle;
    }

    if (HpConsume.HasValue || MpConsume.HasValue) {
      return SkillActivationType.Active;
    }

    return SkillActivationType.Passive;

  }

  public bool IsPassive => GetActivationType() == SkillActivationType.Passive;
  public bool IsToggle => GetActivationType() == SkillActivationType.Toggle;
  public bool IsActive => GetActivationType() == SkillActivationType.Active;

}

