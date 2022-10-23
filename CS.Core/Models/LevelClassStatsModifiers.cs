using CS.Core.Enums;

namespace CS.Core.Models;
public class LevelClassStatsModifiers {

  public CsEntityClass CharacterClass { get; set; }
  public double BaseHpPercentPerLevel { get; set; }
  public double BaseMpPercentPerLevel { get; set; }
  public double BasePhysicalAttack { get; set; }
  public double BasePhysicalDefense { get; set; }

}
