using CS.Core.Enums;

namespace CS.Core.Models;
public class CharacterBaseStats {

  public CsEntityClass CharacterClass { get; set; }
  public double Hp { get; set; }
  public double HpRegeneration { get; set; }
  public double Mp { get; set; }
  public double MpRegeneration { get; set; }

  public double PhysicalAttack { get; set; }
  public double PhysicalAttackSpeed { get; set; }
  public double PhysicalDefense { get; set; }
  public double AttackRange { get; set; }
  public double CastingSpeed { get; set; }

  public double Speed { get; set; }

}
