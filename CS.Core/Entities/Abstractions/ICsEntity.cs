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
  void UpdateStats(Func<Stats, Stats> updateFactory);
  void SetEngaged();
  void SetTraveling();
  void Toggle();


  void ClearAllEventHandlers();
  public event EventHandler<DamageEventArgs>? on_damage_incurred;
  public event EventHandler<DamageEventArgs>? on_damage_received;

  public void ReceiveDamage(DamageEventArgs damageEventArgs);

}
