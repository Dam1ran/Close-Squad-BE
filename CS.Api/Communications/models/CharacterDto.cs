using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Api.Communications.Models;
public class CharacterDto {
  public long Id { get; set; }
  public string Nickname { get; set; } = "";
  public int Level { get; set; }

  public CharacterClass CharacterClass { get; set; }
  public CharacterStatus CharacterStatus { get; set; }

  public uint MaxHP { get; set; }
  public uint HP { get; set; }
  public uint MaxMP { get; set; }
  public uint MP { get; set; }

  public float XP_Percent { get; set; }


  public static CharacterDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      Nickname = character.Nickname.Value,
      Level = character.Level,
      CharacterClass = character.CharacterClass,
      CharacterStatus = character.CharacterStatus,
      MaxHP = character.MaxHP,
      HP = character.HP,
      MaxMP = character.MaxMP,
      MP = character.MP,
      XP_Percent = character.XP_Percent
    };

}
