using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Support;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public class Character : Entity, ICsEntity {
  #nullable disable
  protected Character() { }
  #nullable restore

  public event EventHandler? on_level_changed;
  public event EventHandler? on_zero_hp;

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
  public long XpLost { get; set; }
  private readonly object xpLock = new object();

  public long AddXpPercent(double percent) {
    var amount = (long)Math.Ceiling((Level_Constants.GetMaxExpFor(Level) * percent) * 0.01);
    AddXp(amount);
    return amount;
  }

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
      if (Level != level) {
        on_level_changed?.Invoke(this, EventArgs.Empty);
      }
      Level = level;
      Xp = sum;

    }

  }


  [NotMapped]
  public CharacterStatus CharacterStatus { get; set; }

  private readonly object statsLock = new object();
  public Stats Stats { get; set; } = new();

  public void Tick() {
    Stats.Tick(CanRegenerate());
    Position.Move(Stats.Speed.Current);
  }

  public void UpdateStats(Func<Stats, Stats> updateFactory) {

    lock (statsLock) {
      Stats = updateFactory(Stats);
    }

  }

  private DateTimeOffset _nextAttackTime;
  private bool _attackReady => _nextAttackTime <= DateTimeOffset.UtcNow;
  private readonly object attackLock = new object();
  public void DoAttack(ICsEntity target) {

    lock (attackLock) {
      if(!_attackReady) {
        return;
      }

      var capMultiplier = Stats.PhysicalAttackSpeed.Cap * 500.0;
      var attackTimeMs = Math.Ceiling(capMultiplier / Stats.PhysicalAttackSpeed.Current);
      _nextAttackTime = DateTimeOffset.Now.AddMilliseconds(attackTimeMs);


      // TODO checks for existence, allows, distance, consumables, restrictions

      // TODO formula engine
      var hitAmount = Math.Min(target.Stats.PhysicalDefense.Current - Stats.PhysicalAttack.Current, 0);

      target.UpdateStats((stats) => {
        stats.Hp.AddCurrentAmount(hitAmount);
        return stats;
      });

    }

  }

  public void OnZeroHp(object? sender, EventArgs e) {
    CharacterStatus = CharacterStatus.Dead;

    var lostAmount = AddXpPercent(-4.0); // will be dependent on situation, buffs
    XpLost = lostAmount;

    Position.Stop();

    on_zero_hp?.Invoke(this, EventArgs.Empty);

  }

  public Position Position { get; set; } = new();

  public uint QuadrantIndex { get; set; }
  public long PlayerId { get; private set; }

  public bool CanTravel() =>
    CharacterStatus == CharacterStatus.Awake &&
    Stats.HasHp();

  public bool CanMove() =>
    (CharacterStatus == CharacterStatus.Awake ||
    CharacterStatus == CharacterStatus.Engaged) &&
    Stats.HasHp();

  public bool CanArrive() =>
    CharacterStatus == CharacterStatus.Traveling &&
    Stats.HasHp();

  public bool CanRegenerate() =>
    CharacterStatus != CharacterStatus.Astray &&
    CharacterStatus != CharacterStatus.Dead;

  public void Init() {
    Stats.Hp.on_zero_current += OnZeroHp;
  }

  public IEnumerable<BarShortcut> BarShortcuts { get; set; } = new List<BarShortcut>();

}
