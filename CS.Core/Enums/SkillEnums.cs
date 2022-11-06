namespace CS.Core.Enums;

public enum SkillActivationType {
  Passive = 1,
  Toggle = 2,
  Active = 3,
}

public enum KindType {
  Friendly = 1,
  Enemy = 2,
  All = 3,
}

public enum TargetType {
  Self = 1,
  Target = 2,
  SelfRadius = 3,
  TargetRadius = 4,
}

public enum ApplyType {
  Instant = 1,
  Passive = 2,
  Buff = 3,
  OverTime = 4,
}

public enum EffectorActivation {
  Ongoing = 1,
  // OnHit = 2,
  // Frequency = 3,
  // AccumulativeDmg = 4,
}

public enum EffectStack {
  PhysicalDefenseUp = 1,
  PhysicalDefenseAuraUp = 2,
  // ...
}
