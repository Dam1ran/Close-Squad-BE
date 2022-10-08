using CS.Core.Exceptions;

namespace CS.Core.Enums;
public enum CharacterRace {
  Divine = 1,
  Human,
  Dwarf,
  Orc,
  NightElf
}

public static class CharacterRaceHelper {

  public static CharacterRace GetRace(this CharacterClass characterClass) {
    return characterClass switch
    {
      CharacterClass.Cupid or CharacterClass.Doctor => CharacterRace.Divine,
      CharacterClass.Medium or CharacterClass.Templar => CharacterRace.Human,
      CharacterClass.Handyman => CharacterRace.Dwarf,
      CharacterClass.Berserk or CharacterClass.Seer => CharacterRace.Orc,
      CharacterClass.Assassin or CharacterClass.Occultist => CharacterRace.NightElf,
      _ => throw new DomainValidationException($"Character race not mapped for {nameof(characterClass)} of {characterClass}.")
    };
  }

}
