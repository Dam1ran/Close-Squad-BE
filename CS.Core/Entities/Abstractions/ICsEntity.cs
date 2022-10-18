using CS.Core.ValueObjects;

namespace CS.Core.Entities.Abstractions;
public interface ICsEntity {

  public Stats Stats { get; set; }
  void UpdateStats(Func<Stats, Stats> updateFactory);

}
