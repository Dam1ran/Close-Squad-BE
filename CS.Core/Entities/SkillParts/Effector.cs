using CS.Core.Enums;

namespace CS.Core.Entities;
public class Effector {
  public long EffectorKeyId { get; set; }
  public bool IsTargeted { get; set; }
  public double Radius { get; set; }
  public KindType KindType { get; set; }
  public int CoolDownMs { get; set; }
  public int AvailableForTickSeconds { get; set; }
  public long EffectKeyId { get; set; }
  public Effect? Effect { get; set; }
  public bool IsIndividualPassive() => !IsTargeted && Radius == 0 && AvailableForTickSeconds == 0 && Effect is not null;
}
