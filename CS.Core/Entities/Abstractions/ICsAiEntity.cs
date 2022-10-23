using CS.Core.Enums;

namespace CS.Core.Entities.Abstractions;
public interface ICsAiEntity: ICsEntity {

  public AiAction CurrentAction { get; set; }
  public void MoveTo(double x, double y);
  public void ApproachTarget();
  public void AttackTarget();
  public void FollowTarget();
  public void Die();
  public void TargetSelf();
  public void CancelTarget();

}
