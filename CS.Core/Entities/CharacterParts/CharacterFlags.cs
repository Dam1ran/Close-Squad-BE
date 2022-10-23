using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.ValueObjects;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsInstance, ICsAiEntity {
  public bool CanTravel() =>
    Status == CsEntityStatus.Awake &&
    Stats.HasHp();

  public bool IsAlive() =>
    (Status == CsEntityStatus.Awake ||
    Status == CsEntityStatus.Engaged) &&
    Stats.HasHp();

  public bool CanArrive() =>
    Status == CsEntityStatus.Traveling &&
    Stats.HasHp();

  public bool CanRegenerate() =>
    Status != CsEntityStatus.Astray &&
    Status != CsEntityStatus.Dead;

  public bool CanApproachTarget() =>
    Target is not null &&
    CsInstanceId != Target.CsInstanceId &&
    QuadrantIndex == Target.QuadrantIndex &&
    Target.Status != CsEntityStatus.Astray &&
    Target.Status != CsEntityStatus.Traveling;

}