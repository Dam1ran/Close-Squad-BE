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

  [NotMapped]
  public AiAction CurrentAction { get; set; }

  [NotMapped]
  public CsEntityStatus Status { get; set; }
  protected void OnAiTick() {

    switch (CurrentAction) {

      case AiAction.Attacking: {
        if (!IsAlive() || !CanApproachTarget())
        {
          CurrentAction = AiAction.None;
          Position.Stop();
          break;
        }

        if (IsTargetInRange(Stats.AttackRange.Current)) {
          if (Target!.Status != CsEntityStatus.Dead) {
            DoAttackTarget();
            Position.Stop();
          } else {
            CurrentAction = AiAction.None;
            Position.Stop();
          }
        } else {
          SetDestinationToReachDistance(Stats.AttackRange.Current);
        }

        break;
      }

      case AiAction.Approaching: {

        if (!IsAlive() || !CanApproachTarget() || IsTargetInRange(Stats.AttackRange.Current) || Position.IsAtDestination)
        {
          CurrentAction = AiAction.None;
          Position.Stop();
          break;
        }

        SetDestinationToReachDistance(Stats.AttackRange.Current);
        break;
      }

      case AiAction.Following: {

        if (!IsAlive() || !CanApproachTarget())
        {
          CurrentAction = AiAction.None;
          Position.Stop();
          break;
        }

        if (!IsTargetInRange(FollowDistance)) {
          SetDestinationToReachDistance(FollowDistance);
        }

        break;
      }
      case AiAction.None:
      default: return;
    }


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

  private bool IsTargetInRange(double range) {
    if (Target is null) {
      return false;
    }

    return Position.GetDistance(Target!.Position) <= range;
  }

  private void SetDestinationToReachDistance(double reachDistance) {
    if (Target is null) {
      return;
    }

    var difference = Position.GetDifference(Target.Position);
    var targetsTickDistance = Target.Stats.Speed.Current;
    var reachDistancePosition = difference.SetLength(reachDistance - targetsTickDistance);
    Position.SetDestination(Target.Position.LocationX - reachDistancePosition.LocationX, Target.Position.LocationY - reachDistancePosition.LocationY);
  }

  private DateTimeOffset _nextAttackTime;
  private bool AttackOnCoolDown() {
    if (!_nextAttackTime.IsInFuture()) {
      var capMultiplier = Stats.PhysicalAttackSpeed.Cap * 500.0;
      var attackTimeMs = Math.Ceiling(capMultiplier / Stats.PhysicalAttackSpeed.Current);
      _nextAttackTime = DateTimeOffset.Now.AddMilliseconds(attackTimeMs);
      return false;
    }

    return true;
  }

  private readonly object attackLock = new object();
  private void DoAttackTarget() {

    lock (attackLock) {
      if(Target is null || AttackOnCoolDown()) {
        return;
      }

      if (Target.Target is null) {
        Target.Target = this;
      }

      SetEngaged();
      Target.SetEngaged();

      // TODO formula engine
      var hitAmount = Math.Min(Target.Stats.PhysicalDefense.Current - Stats.PhysicalAttack.Current, 0);

      Target.UpdateStats((stats) => {
        stats.Hp.AddCurrentAmount(hitAmount);
        return stats;
      });

      on_damage_incurred?.Invoke(this, new(Target, hitAmount));
      Target.ReceiveDamage(new DamageEventArgs(this, hitAmount));

    }

  }

  public void Die() {
    Position.Stop();
    Status = CsEntityStatus.Dead;
    CurrentAction = AiAction.None;
  }




  public void TargetSelf() => Target = this;
  public void CancelTarget() => Target = null;

  public Position Position { get; set; } = new();
  public void SetEngaged() {
    SetEngagedTime();
    Status = CsEntityStatus.Engaged;
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
    }

  }

  public void ReceiveDamage(DamageEventArgs damageEventArgs) {
    on_damage_received?.Invoke(this, damageEventArgs);
  }


}
