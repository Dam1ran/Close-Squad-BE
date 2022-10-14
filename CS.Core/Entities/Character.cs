using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Support;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : Entity {
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
  protected byte _gender;
  public byte Gender {

    get => _gender;

    protected set {
      if (value < 0) {
        _gender = 0;
      } else if (value > 100) {
        _gender = 100;
      } else {
        _gender = value;
      }
    }

  }

  public double XpPercent => Level_Constants.GetExpPercentFor(Xp, Level);
  public int Level { get; private set; }
  public long Xp { get; private set; }
  private readonly object xpLock = new object();

  public void AddXpPercent(double percent) => AddXp((long)Math.Ceiling((Level_Constants.GetMaxExpFor(Level) * percent) * 0.01));
  public void AddXp(long value) {

    lock (xpLock) {
      var sum = Xp + value;
      var level = Level;

      while (sum < 0 || sum > Level_Constants.GetMaxExpFor(level)) {
        if (sum < 0) {
          level--;
          if (level < 0) {
            sum = 0;
            level = 0;
            break;
          }
          sum += Level_Constants.GetMaxExpFor(level);
        } else if (sum > Level_Constants.GetMaxExpFor(level)) {
          sum -= Level_Constants.GetMaxExpFor(level);
          level++;
          if (level > 99) {
            sum = Level_Constants.GetMaxExpFor(99);
            level = 99;
            break;
          }
        }
      }

      Level = level;
      Xp = sum;

    }

  }


  [NotMapped]
  public CharacterStatus CharacterStatus { get; set; }

  private readonly object statsLock = new object();
  public CharacterStats CharacterStats { get; set; } = new();
  public void RegenerationTick() => OnStatsUpdate();
  public void UpdateStats(Func<CharacterStats, CharacterStats> updateFactory) => OnStatsUpdate(updateFactory);

  private void OnStatsUpdate(Func<CharacterStats, CharacterStats>? updateFactory = null) {

    lock (statsLock) {
      if (updateFactory is null)
      {
        CharacterStats.RegenerationTick();
      }
      else
      {
        CharacterStats = updateFactory(CharacterStats);
      }
    }

  }

  public void OnZeroHp(object? sender, EventArgs e) {
    CharacterStatus = CharacterStatus.Dead;
    Position.Stop();
  }

  public Position Position { get; set; } = new();

  public uint QuadrantIndex { get; set; }
  public long PlayerId { get; private set; }

  public bool CanTravel() =>
    CharacterStatus == CharacterStatus.Awake &&
    CharacterStats.HasHp();

  public bool CanMove() =>
    (CharacterStatus == CharacterStatus.Awake ||
    CharacterStatus == CharacterStatus.Engaged) &&
    CharacterStats.HasHp();

  public bool CanArrive() =>
    CharacterStatus == CharacterStatus.Traveling &&
    CharacterStats.HasHp();

  public bool IsAllowedToRegenerate() =>
    CharacterStatus != CharacterStatus.Astray &&
    CharacterStatus != CharacterStatus.Dead;

}
