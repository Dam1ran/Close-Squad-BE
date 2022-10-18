using CS.Core.Entities;
using CS.Core.Models;

namespace CS.Application.Services.Implementations;
public class CharacterStatsHelper {
  public List<CharacterBaseStats> CharactersBaseStats { get; set; } = new ();
  public List<LevelClassStatsModifiers> LevelClassesStatsModifiers { get; set; } = new ();

  public void RecalculateStats(object? sender, EventArgs? e = null) {
    if (sender is Character character) {
      var baseStats = CharactersBaseStats.Single(c => c.CharacterClass == character.CharacterClass);
      var levelClassStatsModifier = LevelClassesStatsModifiers.Single(c => c.CharacterClass == character.CharacterClass);

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

      character.Stats.Speed.Cap = 1.0;
      character.Stats.Speed.Base = baseStats.Speed;

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
