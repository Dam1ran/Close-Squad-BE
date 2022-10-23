using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Models;
public class CharacterSimpleDto {
  public long Id { get; set; }
  public string InstanceId { get; set; } = string.Empty;
  public uint QuadrantIndex { get; set; }
  public string Nickname { get; set; } = "";

  public CsEntityClass CharacterClass { get; set; }
  public CsEntityStatus CharacterStatus { get; set; }
  public AiAction AiAction { get; set; }

  public double X { get; set; }
  public double Y { get; set; }
  // equipment


  public static CharacterSimpleDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      InstanceId = character.CsInstanceId,
      QuadrantIndex = character.QuadrantIndex,
      Nickname = character.Nickname.Value,
      CharacterClass = character.Class,
      CharacterStatus = character.Status,
      AiAction = character.CurrentAction,
      X = character.Position.LocationX,
      Y = character.Position.LocationY
    };

}
