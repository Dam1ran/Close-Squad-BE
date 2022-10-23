using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsAiEntity {
  #nullable disable
  protected Character() { }
  #nullable restore

  [NotMapped]
  public string CsInstanceId { get; set; } = Guid.NewGuid().ToString();
  public long PlayerId { get; private set; }

  #region CTOR
  public Character(
    Nickname nickname,
    CsEntityClass characterClass,
    uint startingQuadrantIndex,
    byte gender)
  {
    Nickname = nickname;
    Class = characterClass;
    QuadrantIndex = startingQuadrantIndex;
    Gender = gender;
  }
  #endregion

  public void Init() {
    Stats.Hp.on_zero_current += OnZeroHp;
  }
  public void Tick() {

    CheckTarget();

    OnAiTick();

    Stats.Tick(CanRegenerate());

    Position.MoveTick(Stats.Speed.Current);

    if (!IsEngaged() &&
        Status != CsEntityStatus.Astray &&
        Status != CsEntityStatus.Traveling &&
        Status != CsEntityStatus.Sitting) {

      Status
        = Stats.HasHp()
        ? CsEntityStatus.Awake
        : CsEntityStatus.Dead;
    }

  }

  public uint QuadrantIndex { get; set; }
  public Stats Stats { get; set; } = new();
  private readonly object statsLock = new object();

  public void UpdateStats(Func<Stats, Stats> updateFactory) {

    lock (statsLock) {
      Stats = updateFactory(Stats);
    }

  }

  public IEnumerable<BarShortcut> BarShortcuts { get; set; } = new List<BarShortcut>();

  public void OnZeroHp(object? sender, EventArgs e) {

    var lostAmount = AddXpPercent(-4.0); // will be dependent on situation, buffs
    XpLost = lostAmount;

    Die();

    on_zero_hp?.Invoke(this, EventArgs.Empty);

  }

  [NotMapped]
  public ICsEntity? Target { get; set; }
}
