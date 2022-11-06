using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Extensions;
using CS.Core.Models;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsInstance, ICsAiEntity {

  public const int EngagedTimeoutSeconds = 20;
  public const double FollowDistance = 10.0;
  public const double PhysicalAttackSpeedHitTimeRatio = 0.5;
  public const double MagicalAttackSpeedHitTimeRatio = 0.5;
  public const double CastSpeedDifferenceRatio = 0.8;

  [NotMapped]
  public AiAction CurrentAction { get; set; }

  [NotMapped]
  public CsEntityStatus Status { get; set; }

  private void AiStop() {
    Position.Stop();
    CurrentAction = AiAction.None;
  }
  protected void OnAiTick() {

    switch (CurrentAction) {

      case AiAction.SkillApply: {
        if (!IsAlive() || !CanApproachTarget() || _applyingSkill is null || !_applyingSkill.Skill.CastRange.HasValue) {
          AiStop();
          break;
        }

        if (IsHitOnCoolDown()) {
          Position.Stop();
          break;
        }

        if (!IsTargetInRange(_applyingSkill.Skill.CastRange.Value)) {
          SetDestinationToTargetReachDistance(_applyingSkill.Skill.CastRange.Value);
          break;
        }

        if (Target!.Status == CsEntityStatus.Dead /* && not resurrection related skills*/) {
          AiStop();
          break;
        }

        if (_applyingSkill.IsSkillOnCoolDown())
        {
          var haveRangedAttackIntention = true; //TODO
          if (haveRangedAttackIntention) {
            SetDestinationAwayFromTargetAtDistance(_applyingSkill.Skill.CastRange.Value);
            break;
          }
        }
        else
        {
          Position.Stop();
          DoApplySkillToTarget();
          break;
        }

        break;
      }
      case AiAction.Attacking: {
        if (!IsAlive() || !CanApproachTarget()) {
          AiStop();
          break;
        }

        if (IsHitOnCoolDown()) {
          Position.Stop();
          break;
        }

        if (!IsTargetInAttackRange()) {
          SetDestinationToTargetReachDistance(Stats.AttackRange.Current);
          break;
        }

        if (Target!.Status == CsEntityStatus.Dead) {
          AiStop();
          break;
        }

        if (IsAttackOnCoolDown())
        {
          var haveRangedAttackIntention = true; //TODO
          if (haveRangedAttackIntention) {
            SetDestinationAwayFromTargetAtReachDistance();
            break;
          }
        }
        else
        {
          Position.Stop();
          DoAttackTarget();
          break;
        }

        break;
      }

      case AiAction.Approaching: {

        if (!IsAlive() || !CanApproachTarget() || IsTargetInRange(Stats.AttackRange.Current) || Position.IsAtDestination) {
          AiStop();
          break;
        }

        if (IsHitOnCoolDown()) {
          Position.Stop();
          break;
        }

        SetDestinationToTargetReachDistance(Stats.AttackRange.Current);
        break;
      }

      case AiAction.Following: {

        if (!IsAlive() || !CanApproachTarget()) {
          AiStop();
          break;
        }

        if (IsHitOnCoolDown()) {
          Position.Stop();
          break;
        }

        if (!IsTargetInRange(FollowDistance)) {
          SetDestinationToTargetReachDistance(FollowDistance);
        }

        break;
      }
      case AiAction.None:
      default: return;
    }

  }

  private void SetDestinationAwayFromTargetAtDistance(double distance) {
    if (Target is null) {
      return;
    }

    var difference = Target.Position.GetDifference(Position);
    var targetsTickDistance = Target.Stats.Speed.Current;
    var reachDistancePosition = difference.SetLength(distance - targetsTickDistance);

    Position.SetDestination(
      Math.Clamp(Target.Position.LocationX + reachDistancePosition.LocationX, 0, 100),
      Math.Clamp(Target.Position.LocationY + reachDistancePosition.LocationY, 0, 100));

  }

  private void SetDestinationAwayFromTargetAtReachDistance() {
    if (Target is null) {
      return;
    }

    var difference = Target.Position.GetDifference(Position);
    var targetsTickDistance = Target.Stats.Speed.Current;
    var reachDistancePosition = difference.SetLength(Stats.AttackRange.Current - targetsTickDistance);

    Position.SetDestination(
      Math.Clamp(Target.Position.LocationX + reachDistancePosition.LocationX, 0, 100),
      Math.Clamp(Target.Position.LocationY + reachDistancePosition.LocationY, 0, 100));

  }

  public void CheckTarget() {
    if (
      Target is not null && Target.CsInstanceId != CsInstanceId && (
      Target.Status == CsEntityStatus.Astray ||
      Target.Status == CsEntityStatus.Traveling ||
      Target.QuadrantIndex != QuadrantIndex) ||
      Status == CsEntityStatus.Dead)
    {
      CancelTarget();
    }
  }
  public void MoveTo(double x, double y) {
    // TODO: push to AI switch
    Position.SetDestination(x, y);
    CurrentAction = AiAction.Moving;
  }

  public void ApproachTarget() {
    CurrentAction = AiAction.Approaching;
    Position.IsAtDestination = false;
  }
  public void AttackTarget() {
    CurrentAction = AiAction.Attacking;
    Position.IsAtDestination = false;
  }
  public void FollowTarget() {
    CurrentAction = AiAction.Following;
    Position.IsAtDestination = false;
  }
  [NotMapped]
  private SkillWrapper? _applyingSkill;
  // public void ApplySkillToTarget(SkillWrapper skillWrapper) {
  //   CurrentAction = AiAction.SkillApply;
  //   Position.IsAtDestination = false;
  //   _applyingSkill = skillWrapper;
  // }



  private bool IsTargetInAttackRange() => IsTargetInRange(Stats.AttackRange.Current);
  private bool IsTargetInRange(double range) => Position.GetDistance(Target!.Position) <= range;

  private void SetDestinationToTargetReachDistance(double reachDistance) {
    if (Target is null) {
      return;
    }

    var difference = Position.GetDifference(Target.Position);
    var targetsTickDistance = Target.Stats.Speed.Current;
    var reachDistancePosition = difference.SetLength(reachDistance - targetsTickDistance);
    Position.SetDestination(Target.Position.LocationX - reachDistancePosition.LocationX, Target.Position.LocationY - reachDistancePosition.LocationY);
  }

  private DateTimeOffset _nextAttackTime;
  private bool IsAttackOnCoolDown() => _nextAttackTime.IsInFuture();
  private DateTimeOffset _nextHitTime;
  private bool IsHitOnCoolDown() => _nextHitTime.IsInFuture();
  private void SetAttackAndHitCoolDown() {

    var capMultiplier = Stats.PhysicalAttackSpeed.Cap * 500.0;
    var attackTimeMs = Math.Ceiling(capMultiplier / Stats.PhysicalAttackSpeed.Current);

    _nextAttackTime = DateTimeOffset.Now.AddMilliseconds(attackTimeMs);
    _nextHitTime = DateTimeOffset.Now.AddMilliseconds(attackTimeMs * PhysicalAttackSpeedHitTimeRatio);
  }

  private void SetSkillAndHitCoolDown(SkillWrapper skillWrapper) {
    if (!skillWrapper.Skill.CoolDownMs.HasValue) {
      return;
    }
    var castSpeedCapRatio = Stats.CastingSpeed.Current / Stats.CastingSpeed.Cap;
    var coolDownMs = Math.Ceiling(skillWrapper.Skill.CoolDownMs.Value - skillWrapper.Skill.CoolDownMs.Value * CastSpeedDifferenceRatio * castSpeedCapRatio);
    skillWrapper.CoolDown = DateTimeOffset.Now.AddMilliseconds(coolDownMs);

    _nextHitTime = DateTimeOffset.Now.AddMilliseconds(coolDownMs * MagicalAttackSpeedHitTimeRatio);
  }

  private void DoAttackTarget() {
    if(Target is null) {
      return;
    }


    if (Target.Target is null) {
      Target.Target = this;
    }


    SetEngaged();
    Target.SetEngaged();

    // TODO formula engine
    var hitAmount = Math.Min(Target.Stats.PhysicalDefense.Current - Stats.PhysicalAttack.Current, 0);
    // TODO reflect dmg..
    Target.Stats.UpdateStat(StatType.Hp, StatOperation.AddCurrentAmount, hitAmount);

    SetAttackAndHitCoolDown();

    on_damage_incurred?.Invoke(this, new(Target, hitAmount));
    Target.ReceiveDamage(new DamageEventArgs(this, hitAmount));

  }

  private void DoApplySkillToTarget() {
    if(Target is null || _applyingSkill is null) {
      return;
    }

    if (Target.Target is null) {
      Target.Target = this;
    }

    // var skillEffects = _getEffectsOf!.Invoke(_applyingSkill.Skill);
    // if (skillEffects.Any(se => se.Kind == SkillKindType.Enemy)) {
    //   SetEngaged();
    // }
    // Target.SetEngaged();

    // TODO formula engine
    // var hitAmount = Math.Min(Target.Stats.PhysicalDefense.Current - Stats.PhysicalAttack.Current, 0);
    // // var hitAmount = _applyingSkill.Skill.EffectStats[0].Value; // TODO

    // Target.UpdateStats((stats) => {
    //   stats.Hp.AddCurrentAmount(hitAmount);
    //   return stats;
    // });

    // SetSkillAndHitCoolDown();

    // on_damage_incurred?.Invoke(this, new(Target, hitAmount));
    // Target.ReceiveDamage(new DamageEventArgs(this, hitAmount));

    // if friendly then 
      //   _applyingSkill = null;
    // CurrentAction = AiAction.None;

  }

  public void Die() {
    AiStop();
    Status = CsEntityStatus.Dead;
  }




  public void TargetSelf() => Target = this;
  public void CancelTarget() => Target = null;

  public bool IsSelfTargeted() => Target is not null && CsInstanceId == Target.CsInstanceId;

  public Position Position { get; set; } = new();
  public void SetEngaged() {
    SetEngagedTime();
    if (Status != CsEntityStatus.Astray &&
        Status != CsEntityStatus.Traveling &&
        Status != CsEntityStatus.Dead) {
      Status = CsEntityStatus.Engaged;
    }
  }

  [NotMapped]
  public DateTimeOffset EngagedTill { get; set; }
  public void SetEngagedTime() => EngagedTill = DateTimeOffset.UtcNow.AddSeconds(EngagedTimeoutSeconds);
  public bool IsEngaged() => EngagedTill.IsInFuture();

  public void SetTraveling(){
    Status = CsEntityStatus.Traveling;
    Position.Stop();
  }

  public void Toggle() {

    if (Status == CsEntityStatus.Engaged) {
      return;
    }

    Status
      = Status != CsEntityStatus.Astray
      ? CsEntityStatus.Astray
      : Stats.HasHp()
      ? CsEntityStatus.Awake
      : CsEntityStatus.Dead;

    if (Status == CsEntityStatus.Astray) {
      CancelTarget();
      AiStop();
    }

  }

  public void ReceiveDamage(DamageEventArgs damageEventArgs) {
    on_damage_received?.Invoke(this, damageEventArgs);
  }

  public void UseSkill(SkillWrapper skillWrapper) {

    if (!IsAlive() || IsHitOnCoolDown()) {
      return;
    }

    if (skillWrapper.Skill.IsPassive) {

      Effectors.Add(skillWrapper.Skill.Effectors);

    } else if (skillWrapper.Skill.IsToggle) {

      if (skillWrapper.IsToggleActivated) {
        // Effects.SetExpireEffects(effects); TODO
      } else {
        // SetEffects(effects); TODO
      }

    } else {
      // UseActiveSkill(skillWrapper, effects); TODO
    }

  }

/*
  private void UseActiveSkill(SkillWrapper skillWrapper, IEnumerable<Effect> effects) {
    foreach(var effect in effects) {
      if (effect.TargetType == TargetType.Self)
      {
        if (skillWrapper.IsSkillOnCoolDown() || !Stats.CanBeConsumed(skillWrapper.Skill)) {
          break;
        }

        Stats.Consume(skillWrapper.Skill);
        // TODO inventory consume...
        SetSkillAndHitCoolDown(skillWrapper);

        ApplyEffect(this, effect, true);

      }
      else if (effect.TargetType == TargetType.Target && Target is not null && Target.QuadrantIndex == QuadrantIndex && skillWrapper.Skill.CastRange.HasValue)
      {
        if (effect.Kind == KindType.Enemy && IsSelfTargeted()) {
          break;
        }

        if (!IsTargetInRange(skillWrapper.Skill.CastRange.Value)) {
          SetApplyingSkill(skillWrapper);
          break;
        }

        if (skillWrapper.IsSkillOnCoolDown() || !Stats.CanBeConsumed(skillWrapper.Skill)) {
          break;
        }

        if (effect.Kind == KindType.Enemy) {
          SetEngaged();
          Target.SetEngaged();
        }

        Stats.Consume(skillWrapper.Skill);
        // TODO inventory consume...
        SetSkillAndHitCoolDown(skillWrapper);

        ApplyEffect(Target, effect, true);

      }
      else if (effect.TargetType == TargetType.SelfRadius && effect.EffectRange.HasValue)
      {
        if (skillWrapper.IsSkillOnCoolDown() || !Stats.CanBeConsumed(skillWrapper.Skill)) {
          break;
        }

        if (effect.Kind == KindType.Enemy) {
          SetEngaged();
        }

        var affectedTargets =
          _getCharactersInRadius!.Invoke(QuadrantIndex, Position, effect.EffectRange.Value)
          .Where(t => t.CsInstanceId != CsInstanceId || effect.Kind == KindType.Friendly);

        Stats.Consume(skillWrapper.Skill);
        // TODO inventory consume...
        SetSkillAndHitCoolDown(skillWrapper);

        foreach(var affectedTarget in affectedTargets) {

          ApplyEffect(affectedTarget, effect, true);

          if (effect.Kind == KindType.Enemy) {
            affectedTarget.SetEngaged();
          }

        }

      }
      else if (effect.TargetType == TargetType.TargetRadius && Target is not null && Target.QuadrantIndex == QuadrantIndex && skillWrapper.Skill.CastRange.HasValue && effect.EffectRange.HasValue)
      {
        if (effect.Kind == KindType.Enemy && IsSelfTargeted()) {
          break;
        }

        if (!IsTargetInRange(skillWrapper.Skill.CastRange.Value)) {
          SetApplyingSkill(skillWrapper);
          break;
        }

        if (!Stats.CanBeConsumed(skillWrapper.Skill) || skillWrapper.IsSkillOnCoolDown()) {
          break;
        }

        if (effect.Kind == KindType.Enemy) {
          SetEngaged();
        }

        var affectedTargets =
          _getCharactersInRadius!.Invoke(QuadrantIndex, Target.Position, effect.EffectRange.Value)
          .Where(t => t.CsInstanceId != CsInstanceId || effect.Kind == KindType.Friendly);

        Stats.Consume(skillWrapper.Skill);
        // TODO inventory consume...
        SetSkillAndHitCoolDown(skillWrapper);

        foreach(var affectedTarget in affectedTargets) {

          ApplyEffect(affectedTarget, effect, true);

          if (effect.Kind == KindType.Enemy) {
            affectedTarget.SetEngaged();
          }

        }

      }

    }
  }
*/
  public void SetEffects(IEnumerable<Effect> effects) {

    Stats.ApplyEffects(effects.Where(e => e.ApplyType == ApplyType.Instant));
    Effects.Add(effects.Where(e => e.ApplyType != ApplyType.Instant));

  }


  private void SetApplyingSkill(SkillWrapper skillWrapper) {
    _applyingSkill = skillWrapper;
    CurrentAction = AiAction.SkillApply;
  }


  [NotMapped]
  public Effects Effects { get; set; } = new();
  [NotMapped]
  public Effectors Effectors { get; set; } = new();

}
