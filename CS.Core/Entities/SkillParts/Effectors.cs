using CS.Core.Extensions;

namespace CS.Core.Entities;
public class Effectors {
  private readonly List<EffectorWrapper> _effectors = new();
  private readonly object _effectorsLock = new object();

  public void Add(IEnumerable<Effector> effectors) {
    lock (_effectorsLock) {
      foreach (var effector in effectors) {
        if (_effectors.Any(ec => ec.Effector.EffectorKeyId == effector.EffectorKeyId)) {
          continue;
        }

        _effectors.Add(new(effector));
      }
    }
  }

  public void ClearExpired() {
    lock (_effectorsLock) {
      _effectors.RemoveAll(ew => ew.IsExpired());
    }
  }

  public IEnumerable<EffectorWrapper> GetUnapplied() {

    lock (_effectorsLock) {
      return _effectors.Where(ew => !ew.IsApplied);
    }

  }
  public void SetApplied(EffectorWrapper effectorWrapper) {

    lock (_effectorsLock) {
      var effectorWrapperToSetApplied = _effectors.SingleOrDefault(e => e.Effector.EffectKeyId == effectorWrapper.Effector.EffectKeyId);
      if (effectorWrapperToSetApplied is not null) {
        effectorWrapperToSetApplied.IsApplied = true;
      }
    }

  }

}

public class EffectorWrapper {
  public EffectorWrapper(Effector effector) {
    Effector = effector;

    if (effector.AvailableForTickSeconds > 0) {
      SetConsistentForNSeconds(effector.AvailableForTickSeconds);
    } else {
      SetConsistent();
    }

  }

  public Effector Effector { get; set; }
  public DateTimeOffset EndAt { get; set; }
  public void SetConsistent() => EndAt = DateTimeOffset.MaxValue;
  public void SetExpired() => EndAt = DateTimeOffset.MinValue;
  public void SetConsistentForNSeconds(int seconds) => EndAt = DateTimeOffset.UtcNow.AddSeconds(seconds);
  public bool IsExpired() => !EndAt.IsInFuture();
  public bool IsApplied { get; set; }

}
