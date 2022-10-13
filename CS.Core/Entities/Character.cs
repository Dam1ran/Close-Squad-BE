using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : CharacterStats {
  #nullable disable
  protected Character() { }
  #nullable restore
  public Character(
    Nickname nickname,
    CharacterClass characterClass,
    uint startingQuadrantIndex,
    byte gender)
  {
    Nickname = nickname;
    CharacterClass = characterClass;
    QuadrantIndex = startingQuadrantIndex;
    Gender = gender;
  }

  public Nickname Nickname { get; private set; }
  public CharacterClass CharacterClass { get; set; }


  [NotMapped]
  public CharacterStatus CharacterStatus { get; set; }

  public Position Position { get; set; } = new();

  public uint QuadrantIndex { get; set; }
  public long PlayerId { get; private set; }

  public bool CanTravel() =>
    CharacterStatus == CharacterStatus.Awake &&
    HpStat.Current > 0;

  public bool CanMove() =>
    (CharacterStatus == CharacterStatus.Awake ||
    CharacterStatus == CharacterStatus.Engaged) &&
    HpStat.Current > 0;

  public bool CanArrive() =>
    CharacterStatus == CharacterStatus.Traveling &&
    HpStat.Current > 0;


}
