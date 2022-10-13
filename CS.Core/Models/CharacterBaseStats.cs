using CS.Core.Entities;

namespace CS.Core.Models;
public class CharacterBaseStats {
  public long Hp { get; set; }
  public long Mp { get; set; }
  public long Speed { get; set; }

  public void AssignBaseStats(Character character, ClassStatsModifiers classStatsModifiers) {
    character.HpStat.Base = (long)(Hp * classStatsModifiers.Hp);
    character.MpStat.Base = (long)(Mp * classStatsModifiers.Mp);
    character.SpeedStat.Base = (long)(Speed * classStatsModifiers.Speed);
  }

}
