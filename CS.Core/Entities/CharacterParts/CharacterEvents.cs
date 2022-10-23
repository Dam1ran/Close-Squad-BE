using CS.Core.Entities.Abstractions;
using CS.Core.Models;

namespace CS.Core.Entities;
public partial class Character : Entity, ICsEntity, ICsInstance, ICsAiEntity {

  public event EventHandler? on_level_changed;
  public event EventHandler? on_zero_hp;

  public event EventHandler<DamageEventArgs>? on_damage_incurred;
  public event EventHandler<DamageEventArgs>? on_damage_received;

  public void ClearAllEventHandlers() {
    if (on_level_changed is not null) {
      foreach(var del in on_level_changed.GetInvocationList()) {
        on_level_changed -= (EventHandler)del;
      }
    }

    if (on_zero_hp is not null) {
      foreach(var del in on_zero_hp.GetInvocationList()) {
        on_zero_hp -= (EventHandler)del;
      }
    }

    if (on_damage_incurred is not null) {
      foreach(var del in on_damage_incurred.GetInvocationList()) {
        on_damage_incurred -= (EventHandler<DamageEventArgs>)del;
      }
    }

    if (on_damage_received is not null) {
      foreach(var del in on_damage_received.GetInvocationList()) {
        on_damage_received -= (EventHandler<DamageEventArgs>)del;
      }
    }

  }
}
