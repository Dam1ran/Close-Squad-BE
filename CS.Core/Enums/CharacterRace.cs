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

  public static CharacterRace GetRace(this CsEntityClass characterClass) {
    return characterClass switch
    {
      CsEntityClass.Cupid or CsEntityClass.Doctor => CharacterRace.Divine,
      CsEntityClass.Medium or CsEntityClass.Templar => CharacterRace.Human,
      CsEntityClass.Handyman => CharacterRace.Dwarf,
      CsEntityClass.Berserk or CsEntityClass.Seer => CharacterRace.Orc,
      CsEntityClass.Assassin or CsEntityClass.Occultist => CharacterRace.NightElf,
      _ => throw new DomainValidationException($"Character race not mapped for {nameof(characterClass)} of {characterClass}.")
    };
  }

}
