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

  public double MaxHp { get; set; }
  public double Hp { get; set; }
  public double MaxMp { get; set; }
  public double Mp { get; set; }

  public double X { get; set; }
  public double Y { get; set; }

  public double XpPercent { get; set; }


  public static CharacterDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      QuadrantIndex = character.QuadrantIndex,
      Nickname = character.Nickname.Value,
      Level = character.Level,
      CharacterClass = character.CharacterClass,
      CharacterStatus = character.CharacterStatus,
      MaxHp = character.CharacterStats.Hp.Max,
      Hp = character.CharacterStats.Hp.Current,
      MaxMp = character.CharacterStats.Mp.Max,
      Mp = character.CharacterStats.Mp.Current,
      XpPercent = character.XpPercent,
      X = character.Position.LocationX,
      Y = character.Position.LocationY
    };

}
