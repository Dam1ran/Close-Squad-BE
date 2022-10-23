using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Application.Models;
public class TargetDto {

  public long CharacterId { get; set; }
  public string? InstanceId { get; set; }
  public string Nickname { get; set; } = string.Empty;
  public double Hp { get; set; }
  public double MaxHp { get; set; }
  public double Mp { get; set; }
  public double MaxMp { get; set; }

  public CsEntityStatus? Status { get; set; }
  public CsEntityClass? CharacterClass { get; set; }

  public static TargetDto? FromCharacter(Character character) {
    if (character.Target is null) {
      return null;
    }

    var result = new TargetDto();

    result.CharacterId = character.Id;
    result.InstanceId =  character.Target.CsInstanceId;

    result.Nickname =  character.Target.Nickname;

    result.Status = character.Target.Status;
    result.CharacterClass = character.Target.Class;

    if (character.Target is Character targetedCharacter) {
      if (targetedCharacter.PlayerId == character.PlayerId /*||  targetedCharacter is in party */) {
        AssignVitals(result, targetedCharacter.Stats);
      }
    }

    // if (character.Target is Creature targetedCreature) {
    //   AssignVitals(result, targetedCreature.Stats);
    // }

    return result;
  }

  private static void AssignVitals(TargetDto targetDto, Stats stats) {
    targetDto.Hp = stats.Hp.Current;
    targetDto.MaxHp = stats.Hp.Max;
    targetDto.Mp = stats.Mp.Current;
    targetDto.MaxMp = stats.Mp.Max;
  }

}
