using CS.Core.Entities;
using CS.Core.Enums;

namespace CS.Application.Models;
public class CharacterDto {
  public long Id { get; set; }
  public string InstanceId { get; set; } = string.Empty;
  public uint QuadrantIndex { get; set; }
  public string Nickname { get; set; } = "";
  public int Level { get; set; }

  public CsEntityClass CharacterClass { get; set; }
  public CsEntityStatus CharacterStatus { get; set; }
  public AiAction AiAction { get; set; }

  public double MaxHp { get; set; }
  public double Hp { get; set; }
  public double MaxMp { get; set; }
  public double Mp { get; set; }

  public double X { get; set; }
  public double Y { get; set; }

  public double XpPercent { get; set; }

  public TargetDto? Target { get; set; }


  public static CharacterDto FromCharacter(Character character) =>
    new() {
      Id = character.Id,
      InstanceId = character.CsInstanceId,
      QuadrantIndex = character.QuadrantIndex,
      Nickname = character.Nickname.Value,
      Level = character.Level,
      CharacterClass = character.Class,
      CharacterStatus = character.Status,
      AiAction = character.CurrentAction,
      MaxHp = character.Stats.Hp.Max,
      Hp = character.Stats.Hp.Current,
      MaxMp = character.Stats.Mp.Max,
      Mp = character.Stats.Mp.Current,
      XpPercent = character.XpPercent,
      X = character.Position.LocationX,
      Y = character.Position.LocationY,
      Target = TargetDto.FromCharacter(character)
    };

}
