using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
    Stats.Init();
    Stats.Hp.on_zero_current += OnZeroHp;
  }
  private Func<uint, Position, double, IEnumerable<ICsEntity>>? _getCharactersInRadius;
  public void SetSearchCharacterInRadiusHandle(Func<uint, Position, double, IEnumerable<ICsEntity>> getCharactersInRadius) {
    _getCharactersInRadius = getCharactersInRadius;
  }

  public void Tick() {

    if (Status == CsEntityStatus.Astray) {
      return;
    }

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

  public void OnSecondTick() {

    if (Status == CsEntityStatus.Astray || Status == CsEntityStatus.Traveling) {
      return;
    }

    Effectors.ClearExpired();

    var effectorWrappers = Effectors.GetUnapplied();

    foreach(var effectorWrapper in effectorWrappers) {
      // TODO Friendly/Enemy/All
      var targets = GetTargetsOf(effectorWrapper.Effector);
      foreach (var target in targets) {
        if (effectorWrapper.Effector.Effect is null) {
          continue;
        }

        target.SetEffects(new [] { effectorWrapper.Effector.Effect });
      }

      if (effectorWrapper.Effector.IsIndividualPassive()) {
        Effectors.SetApplied(effectorWrapper);
      }

    }


    Effects.ClearExpiredAndUnapplied();

    var expiredEffects = Effects.GetExpired();
    foreach(var expiredEffect in expiredEffects) {
      Stats.ClearEffect(expiredEffect);
    }
    Effects.ClearExpired();

    var unappliedEffectWrappers = Effects.GetUnapplied();
    foreach(var unappliedEffectWrapper in unappliedEffectWrappers) {

      Stats.ApplyEffect(unappliedEffectWrapper.Effect);

      if (unappliedEffectWrapper.Effect.ApplyType != ApplyType.OverTime) {
        Effects.SetApplied(unappliedEffectWrapper);
      }

    }

  }

  private IEnumerable<ICsEntity> GetTargetsOf(Effector effector) {

    if (effector.IsTargeted && Target is null) {
      return Enumerable.Empty<ICsEntity>();
    }

    var target = effector.IsTargeted ? Target! : this;

    if (effector.Radius == 0) {
      return new[] { target };
    }

    return _getCharactersInRadius!(QuadrantIndex, target.Position, effector.Radius);

  }


/*

    var toggledSkillWrappers = SkillWrappers.Where(sw => sw.IsToggleActivated);
    foreach (var toggledSkillWrapper in toggledSkillWrappers) {

      if (Stats.CanUseToggleSkill(toggledSkillWrapper.Skill))
      {
        Stats.ConsumeTick(toggledSkillWrapper.Skill);
      }
      else
      {
        var skillEffects = _getEffectsOf!.Invoke(toggledSkillWrapper.Skill);
        Effects.SetExpireEffects(skillEffects);
        toggledSkillWrapper.IsToggleActivated = false;
      }

    }

    Effects.ClearExpiredAndUnapplied();

    // un-apply and clear expired
    var expiredEffects = Effects.GetExpired();
    foreach(var expiredEffect in expiredEffects) {
      Stats.ClearEffect(expiredEffect);
    }
    Effects.ClearExpired();


    var unappliedEffectWrappers = Effects.GetUnapplied();
    foreach(var unappliedEffectWrapper in unappliedEffectWrappers) {

      Stats.ApplyEffect(unappliedEffectWrapper.Effect);

      if (unappliedEffectWrapper.Effect.ApplyType != ApplyType.OverTime) {
        Effects.SetApplied(unappliedEffectWrapper);
      }

    }

*/

  public uint QuadrantIndex { get; set; }
  public Stats Stats { get; set; } = new();

  public List<BarShortcut> BarShortcuts { get; set; } = new List<BarShortcut>();

  public void OnZeroHp(object? sender, EventArgs e) {

    var lostAmount = AddXpPercent(-4.0); // will be dependent on situation, buffs
    XpLost = lostAmount;

    Die();

    on_zero_hp?.Invoke(this, EventArgs.Empty);

  }

  [NotMapped]
  public ICsEntity? Target { get; set; }

  public List<SkillWrapper> SkillWrappers { get; set; } = new();

}
