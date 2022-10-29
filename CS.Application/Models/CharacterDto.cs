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
  public List<StatDto> Stats { get; set; } = new();
  public IEnumerable<long> SkillIds { get; set; } = new List<long>();


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
      Target = TargetDto.FromCharacter(character),
      Stats = new List<StatDto>() {
        new() { Name = "P.Attack", Value = character.Stats.PhysicalAttack.Current },
        new() { Name = "M.Attack", Value = 12 },
        new() { Name = "Speed", Value = character.Stats.Speed.Current * 1000 },
        new() { Name = "A.Range", Value = character.Stats.AttackRange.Current },
        new() { Name = "P.Defense", Value = character.Stats.PhysicalDefense.Current },
        new() { Name = "M.Defense", Value = 5 },
        new() { Name = "Hp.Regen", Value = character.Stats.Hp.RegenerationAmountPerTick },
        new() { Name = "Mp.Regen", Value = character.Stats.Mp.RegenerationAmountPerTick },
      },
      SkillIds = character.SkillWrappers.Select(sw => sw.SkillKeyId) // temp
    };

}
