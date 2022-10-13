using System.ComponentModel.DataAnnotations.Schema;

namespace CS.Core.Entities.Abstractions;
public class Stat {

  private long _base;
  [NotMapped]
  public long Base {
    get => _base;
    set {
      _base = value;
      _maxCalculated = value;
      if (_currentCalculated < 0) {
        _currentCalculated = value;
      }
    }
  }
  private double _maxModifierPercent;
  private double _maxModifierAmount;
  private long _maxCalculated;
  public long Max { get => _maxCalculated; }
  private long _currentCalculated;
  public long Current { get => _currentCalculated; }

  private readonly object maxModifierPercentLock = new object();
  private readonly object maxModifierAmountLock = new object();
  private readonly object maxCalculatedLock = new object();

  private readonly object currentLock = new object();

  public void AddMaxPercent(double maxPercent) {
    lock (maxModifierPercentLock) {
      _maxModifierPercent += maxPercent;
    }

    RecalculateMax();
  }

  public void AddMaxAmount(double maxAmount) {
    lock (maxModifierAmountLock) {
      _maxModifierAmount += maxAmount;
    }

    RecalculateMax();
  }

  private void RecalculateMax() {
    lock (maxCalculatedLock) {
      _maxCalculated = (long)Math.Floor(_base + _base * _maxModifierPercent * 0.01 + _maxModifierAmount);
    }
  }

  public void AddCurrentPercentage(double percent) {

    lock (currentLock) {
      AddCurrentAmount((long)Math.Floor(_base * percent * 0.01));
    }

  }

  public void AddCurrentAmount(long amount) {

    var sumValue = _currentCalculated;
    Interlocked.Add(ref sumValue, amount);
    if (sumValue >= Max) {
      Interlocked.Exchange(ref _currentCalculated, Max);
    } else if (sumValue <= 0) {
      Interlocked.Exchange(ref _currentCalculated, 0);
    } else {
      Interlocked.Exchange(ref _currentCalculated, sumValue);
    }

  }

}
