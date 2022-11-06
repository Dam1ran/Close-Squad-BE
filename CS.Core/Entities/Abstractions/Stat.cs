using System.ComponentModel.DataAnnotations.Schema;
using CS.Core.Enums;
using CS.Core.Exceptions;

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

  [NotMapped]
  public double Cap { get; set; }
  public double Max { get => Math.Min(_maxCalculated, Cap); }
  public double Current { get; private set; } = -1;

  [NotMapped]
  public double RegenerationAmountPerTick { get; set; }

  private void AddMaxPercent(double maxPercent) {
    _maxModifierPercent += maxPercent;

    RecalculateMax();
  }

  private void AddMaxAmount(double maxAmount) {
    _maxModifierAmount += maxAmount;

    RecalculateMax();
  }

  private void RecalculateMax() => _maxCalculated = _base + _base * _maxModifierPercent * 0.01 + _maxModifierAmount;

  private void AddCurrentByMaxPercent(double percent) => AddCurrentAmount(_maxCalculated * percent * 0.01);
  private void SetCurrentByPercent(double percent) => Current = Math.Max(Max * percent * 0.01, 0);
  private void AddCurrentByPercent(double percent) => AddCurrentAmount(Current * percent * 0.01);

  private void AddCurrentAmount(double amount) {

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

  public void Tick() => AddCurrentAmount(RegenerationAmountPerTick);

  private readonly object _statLock = new object();

  public void Set(StatOperation statOperation, double value) {
    lock (_statLock) {
      switch (statOperation)
      {
        case StatOperation.AddMaxPercent: {
          AddMaxPercent(value);
          break;
        }
        case StatOperation.AddMaxAmount: {
          AddMaxAmount(value);
          break;
        }
        case StatOperation.AddCurrentByMaxPercent: {
          AddCurrentByMaxPercent(value);
          break;
        }
        case StatOperation.SetCurrentByPercent: {
          SetCurrentByPercent(value);
          break;
        }
        case StatOperation.AddCurrentByPercent: {
          AddCurrentByPercent(value);
          break;
        }
        case StatOperation.AddCurrentAmount: {
          AddCurrentAmount(value);
          break;
        }
        case StatOperation.AddMaxAndCurrentAmount: {
          AddMaxAmount(value);
          AddCurrentAmount(value);
          break;
        }
        default: throw new DomainValidationException($"{nameof(Stat)}: Case not defined for {nameof(StatOperation)}.{statOperation}.");
      }
    }
  }
}
