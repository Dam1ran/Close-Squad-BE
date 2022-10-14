using CS.Core.Entities;

namespace CS.Core.Models;
public class CharacterBaseStats {
  public double Hp { get; set; }
  public double HpRegeneration { get; set; }
  public double Mp { get; set; }
  public double MpRegeneration { get; set; }
  public double Speed { get; set; }

  public void AssignBaseStats(Character character, ClassStatsModifiers classStatsModifiers) {

    character.CharacterStats.Hp.Base = Hp * classStatsModifiers.Hp;
    character.CharacterStats.Hp.RegenerationAmountPerTick = HpRegeneration * classStatsModifiers.HpRegeneration;

    character.CharacterStats.Mp.Base = Mp * classStatsModifiers.Mp;
    character.CharacterStats.Mp.RegenerationAmountPerTick = MpRegeneration * classStatsModifiers.MpRegeneration;

    character.CharacterStats.Speed.Base = Speed * classStatsModifiers.Speed;

  }

}
