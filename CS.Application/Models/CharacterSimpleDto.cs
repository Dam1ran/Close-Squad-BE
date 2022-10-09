using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Models;
public class CharacterSimpleDto {
  public long Id { get; set; }
  public uint QuadrantIndex { get; set; }
  public string Nickname { get; set; } = "";
  public int Level { get; set; }

  public CharacterClass CharacterClass { get; set; }
  public CharacterStatus CharacterStatus { get; set; }

  // equipment


  public static CharacterSimpleDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      QuadrantIndex = character.QuadrantIndex,
      Nickname = character.Nickname.Value,
      Level = character.Level,
      CharacterClass = character.CharacterClass,
      CharacterStatus = character.CharacterStatus,
    };

}
