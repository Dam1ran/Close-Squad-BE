using CS.Core.Entities.Abstractions;

namespace CS.Core.Models;
public class DamageEventArgs: EventArgs {
  public DamageEventArgs(ICsEntity csEntity, double damage) {
    CsEntity = csEntity;
    Damage = damage;
  }

  public ICsEntity CsEntity { get; set; }
  public double Damage { get; set; }

}
