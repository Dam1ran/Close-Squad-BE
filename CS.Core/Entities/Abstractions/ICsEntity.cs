using CS.Core.Enums;
using CS.Core.Models;
using CS.Core.ValueObjects;

namespace CS.Core.Entities.Abstractions;
public interface ICsEntity: ICsInstance, IStats, IPosition {
  public Nickname Nickname { get; protected set; }
  public uint QuadrantIndex { get; set; }
  public CsEntityStatus Status { get; set; }
  public CsEntityClass Class { get; set; }
  public ICsEntity? Target { get; set; }
  public List<SkillWrapper> SkillWrappers { get; set; }

  public void SetEffects(IEnumerable<Effect> effects);
  public Effects Effects { get; set; }

  void SetEngaged();
  void SetTraveling();
  void Toggle();
  void UseSkill(SkillWrapper skillWrapper);


  void ClearAllEventHandlers();
  public event EventHandler<DamageEventArgs>? on_damage_incurred;
  public event EventHandler<DamageEventArgs>? on_damage_received;

  public void ReceiveDamage(DamageEventArgs damageEventArgs);

}
