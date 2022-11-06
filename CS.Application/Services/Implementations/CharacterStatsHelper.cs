using System.Reflection;
using CS.Core.Entities;
using CS.Core.Exceptions;
using CS.Core.Models;
using Newtonsoft.Json;

namespace CS.Application.Services.Implementations;
public class CharacterStatsHelper {
  public List<CharacterBaseStats> CharactersBaseStats { get; set; } = new ();
  public List<LevelClassStatsModifiers> LevelClassesStatsModifiers { get; set; } = new ();

    public void Init() {

    var charactersBaseStatsPath = Path.Combine(Path.GetDirectoryName(
      Assembly.GetExecutingAssembly().Location)!,
      "Files/Character",
      $"CharactersBaseStats.json");

    using var charactersBaseStatsReader = new StreamReader(charactersBaseStatsPath);
    CharactersBaseStats
      = JsonConvert.DeserializeObject<List<CharacterBaseStats>>(charactersBaseStatsReader.ReadToEnd())
      ?? throw new NotFoundException("CharactersBaseStats.json could not be loaded.");

    charactersBaseStatsReader.Close();
    charactersBaseStatsReader.Dispose();

    var levelClassesStatsModifiersPath = Path.Combine(Path.GetDirectoryName(
      Assembly.GetExecutingAssembly().Location)!,
      "Files/Character",
      $"LevelClassStatsModifiers.json");

    using var classesStatsModifiersReader = new StreamReader(levelClassesStatsModifiersPath);
    LevelClassesStatsModifiers
      = JsonConvert.DeserializeObject<List<LevelClassStatsModifiers>>(classesStatsModifiersReader.ReadToEnd())
      ?? throw new NotFoundException("ClassStatsModifiers.json could not be loaded.");

    classesStatsModifiersReader.Close();
    classesStatsModifiersReader.Dispose();

  }

  public void RecalculateStats(object? sender, EventArgs? e = null) {
    if (sender is Character character) {
      var baseStats = CharactersBaseStats.Single(c => c.CharacterClass == character.Class);
      var levelClassStatsModifier = LevelClassesStatsModifiers.Single(c => c.CharacterClass == character.Class);

      // dependent on level
      character.Stats.Hp.Base
        = GetValueByPercentAtStep(baseStats.Hp, levelClassStatsModifier.BaseHpPercentPerLevel, character.Level);
      character.Stats.Hp.Cap = 30000; // ????

      character.Stats.Mp.Base
        = GetValueByPercentAtStep(baseStats.Mp, levelClassStatsModifier.BaseMpPercentPerLevel, character.Level);
      character.Stats.Mp.Cap = 30000; // ????

      character.Stats.PhysicalAttack.Base
        = GetValueByPercentAtStep(baseStats.PhysicalAttack, levelClassStatsModifier.BasePhysicalAttack, character.Level);
      character.Stats.PhysicalAttack.Cap = 10000; // ????

      character.Stats.PhysicalDefense.Base
        = GetValueByPercentAtStep(baseStats.PhysicalDefense, levelClassStatsModifier.BasePhysicalDefense, character.Level);
      character.Stats.PhysicalDefense.Cap = 10000; // ????


      // somewhat static
      character.Stats.Hp.RegenerationAmountPerTick = baseStats.HpRegeneration;
      character.Stats.Mp.RegenerationAmountPerTick = baseStats.MpRegeneration;

      character.Stats.PhysicalAttackSpeed.Cap = 1000; // ????
      character.Stats.PhysicalAttackSpeed.Base = baseStats.PhysicalAttackSpeed;

      character.Stats.AttackRange.Cap = 30.0; // ????
      character.Stats.AttackRange.Base = baseStats.AttackRange;

      character.Stats.Speed.Cap = 1.0;
      character.Stats.Speed.Base = baseStats.Speed;

      character.Stats.CastingSpeed.Cap = 1000; // ????
      character.Stats.CastingSpeed.Base = baseStats.CastingSpeed;
    }
  }
  public void ConnectHandlersAndInit(Character character) {
    character.Init();
    character.on_level_changed += RecalculateStats;
  }


  private double GetValueByPercentAtStep(double initialValue, double percent, int step) {
    for (var i = 0 ; i < step; i++) {
      initialValue += initialValue * percent;
    }
    return initialValue;
  }

}
