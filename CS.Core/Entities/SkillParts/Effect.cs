using CS.Core.Enums;

namespace CS.Core.Entities;
public class Effect {
  public long EffectKeyId { get; set; }

  public EffectStack EffectStack { get; set; }
  public int Level { get; set; }


  public ApplyType ApplyType { get; set; }

  public StatType StatType { get; set; }
  public StatOperation StatOperation { get; set; }
  public double Value { get; set; }

  public int? AvailableForTickSeconds { get; set; }
  // public double? EffectRange { get; set; }
}
