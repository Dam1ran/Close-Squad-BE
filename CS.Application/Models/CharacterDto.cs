using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Models;
public class CharacterDto {
  public long Id { get; set; }
  public uint QuadrantIndex { get; set; }
  public string Nickname { get; set; } = "";
  public int Level { get; set; }

  public CharacterClass CharacterClass { get; set; }
  public CharacterStatus CharacterStatus { get; set; }

  public long MaxHP { get; set; }
  public long HP { get; set; }
  public long MaxMP { get; set; }
  public long MP { get; set; }

  public double X { get; set; }
  public double Y { get; set; }

  public float XP_Percent { get; set; }


  public static CharacterDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      QuadrantIndex = character.QuadrantIndex,
      Nickname = character.Nickname.Value,
      Level = character.Level,
      CharacterClass = character.CharacterClass,
      CharacterStatus = character.CharacterStatus,
      MaxHP = character.HpStat.Max,
      HP = character.HpStat.Current,
      MaxMP = character.MpStat.Max,
      MP = character.MpStat.Current,
      // XP_Percent = character.XP_Percent,
      X = character.Position.LocationX,
      Y = character.Position.LocationY
    };

}
