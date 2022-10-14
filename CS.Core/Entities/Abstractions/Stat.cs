using System.ComponentModel.DataAnnotations.Schema;

namespace CS.Core.Entities.Abstractions;
public class Stat {
  public event EventHandler? on_zero_current;
  private double _base;
  [NotMapped]
  public double Base {
    get => _base;
    set {
      _base = value;
      _maxCalculated = value;
      if (Current < 0) {
        Current = value;
      }
    }
  }
  private double _maxModifierPercent;
  private double _maxModifierAmount;
  private double _maxCalculated;
  public double Max { get => _maxCalculated; }
  public double Current { get; private set; }

  [NotMapped]
  public double RegenerationAmountPerTick { get; set; }

  public void AddMaxPercent(double maxPercent) {
    _maxModifierPercent += maxPercent;

    RecalculateMax();
  }

  public void AddMaxAmount(double maxAmount) {
    _maxModifierAmount += maxAmount;

    RecalculateMax();
  }

  private void RecalculateMax() => _maxCalculated = _base + _base * _maxModifierPercent * 0.01 + _maxModifierAmount;

  public void AddCurrentByMaxPercent(double percent) => AddCurrentAmount(_maxCalculated * percent * 0.01);
  public void SetCurrentByPercent(double percent) => Current = Max * percent * 0.01;
  public void AddCurrentByPercent(double percent) => AddCurrentAmount(Current * percent * 0.01);

  public void AddCurrentAmount(double amount) {

    var sumValue = Current + amount;
    if (sumValue >= Max) {
      Current = Max;
    } else if (sumValue <= 0) {
      Current = 0;
      on_zero_current?.Invoke(this, EventArgs.Empty);
    } else {
      Current = sumValue;
    }

  }

  public void RegenerationTick() => AddCurrentAmount(RegenerationAmountPerTick);

}
