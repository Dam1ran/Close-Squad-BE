using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : EntityStats {
  #nullable disable
  protected Character() { }
  #nullable restore
  public Character(
    Quadrant quadrant,
    Nickname nickname,
    CharacterRace characterRace,
    CharacterClass characterClass,
    byte gender)
  {
    Quadrant = quadrant;
    Nickname = nickname;
    CharacterRace = characterRace;
    CharacterClass = characterClass;
    Gender = gender;
  }

  public Nickname Nickname { get; private set; }
  public CharacterRace CharacterRace { get; set; }
  public CharacterClass CharacterClass { get; set; }

  public bool IsAwake { get; set; }
  public Quadrant Quadrant { get; set; }
  public Player Player { get; set; } = null!;

}
