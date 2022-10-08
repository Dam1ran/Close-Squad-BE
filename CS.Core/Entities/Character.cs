using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : EntityStats {
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

  public uint QuadrantIndex { get; set; }
  public long PlayerId { get; private set; }
  public Player Player { get; set; } = null!;

  public bool CanTravel() =>
    CharacterStatus == CharacterStatus.Awake &&
    HP > 0;

  public bool CanArrive() =>
    CharacterStatus == CharacterStatus.Traveling &&
    HP > 0;

}
