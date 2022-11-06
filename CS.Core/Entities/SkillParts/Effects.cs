using CS.Core.Entities.Abstractions;
using CS.Core.Enums;
using CS.Core.Exceptions;
using CS.Core.Extensions;

namespace CS.Core.Entities;
public class Effects {
  private readonly List<EffectWrapper> _effects = new();
  private readonly object _effectsLock = new object();


  public void Add(IEnumerable<Effect> effects) {
    lock (_effectsLock) {
      foreach (var effect in effects) {
        Add(effect);
      }
    }
  }

  private void Add(Effect effect) {

    var sameTypeEffectWrappers = _effects.Where(e => e.Effect.EffectStack == effect.EffectStack);
    // add new
    if (!sameTypeEffectWrappers.Any()) {
      _effects.Add(new(effect));
      return;
    }

    // do nothing - strongerEffectWrapper present
    if (sameTypeEffectWrappers.Any(ew => ew.Effect.Level > effect.Level)) {
      return;
    }

    // refresh timeout
    var sameEffectWrapper = sameTypeEffectWrappers.SingleOrDefault(ew => ew.Effect.Level == effect.Level);
    if (sameEffectWrapper is not null && effect.AvailableForTickSeconds.HasValue) {

      // instant should already been applied

      // passive is already consistent

      // refreshing for buff and overtime
      if (sameEffectWrapper.Effect.ApplyType == ApplyType.Buff ||
          sameEffectWrapper.Effect.ApplyType == ApplyType.OverTime)
      {
        sameEffectWrapper.SetConsistentForNSeconds(effect.AvailableForTickSeconds.Value);
      }

      return;
    }

    // set to replace
    var lesserEffectWrappers = sameTypeEffectWrappers.Where(ew => ew.Effect.Level < effect.Level);
    foreach(var lesserEffect in lesserEffectWrappers) {
      lesserEffect.SetExpired();
    }

    _effects.Add(new(effect));

  }

  // public void SetExpireEffect(Effect effect) {
  //   lock (_effectsLock) {
  //     var effectToExpire = _effects.SingleOrDefault(e => e.Effect.EffectKeyId == effect.EffectKeyId);
  //     if (effectToExpire is not null) {
  //       effectToExpire.SetExpired();
  //     }
  //   }
  // }
  public void SetExpireEffects(IEnumerable<Effect> effects) {
    lock (_effectsLock) {
      var effectsToExpire = _effects.Where(ew => effects.Any(e => e.EffectKeyId == ew.Effect.EffectKeyId));
      foreach(var effectToExpire in effectsToExpire) {
        effectToExpire.SetExpired();
      }
    }
  }

  public void ClearExpiredAndUnapplied() {
    lock (_effectsLock) {
      _effects.RemoveAll(ew => ew.IsExpired() && !ew.IsApplied);
    }
  }

  public IEnumerable<Effect> GetExpired() {
    lock (_effectsLock) {
      return _effects.Where(ew => ew.IsExpired()).Select(ew => ew.Effect);
    }
  }
  public IEnumerable<EffectWrapper> GetUnapplied() {
    lock (_effectsLock) {
      return _effects.Where(ew => !ew.IsApplied);
    }
  }

  public void SetApplied(EffectWrapper effectWrapper) {
    lock (_effectsLock) {
      var effectToSetApplied = _effects.SingleOrDefault(e => e.Effect.EffectKeyId == effectWrapper.Effect.EffectKeyId);
      if (effectToSetApplied is not null) {
        effectToSetApplied.IsApplied = true;
      }
    }
  }

  public void ClearExpired() {
    lock (_effectsLock) {
      _effects.RemoveAll(ew => ew.IsExpired());
    }
  }

}

public class EffectWrapper {
  public EffectWrapper(Effect effect) {

    if (effect.ApplyType == ApplyType.Instant) {
      throw new DomainValidationException($"{nameof(EffectWrapper)}: Cannot be created with {nameof(ApplyType)}.{ApplyType.Instant} effect.");
    }

    Effect = effect;

    if (effect.ApplyType == ApplyType.Passive)
    {
      SetConsistent();
    }
    else if (effect.AvailableForTickSeconds.HasValue &&
      (effect.ApplyType == ApplyType.Buff || effect.ApplyType == ApplyType.OverTime))
    {
      SetConsistentForNSeconds(effect.AvailableForTickSeconds.Value);
    }

  }

  public Effect Effect { get; set; }
  public DateTimeOffset EndAt { get; set; }
  public void SetConsistent() => EndAt = DateTimeOffset.MaxValue;
  public void SetExpired() => EndAt = DateTimeOffset.MinValue;
  public void SetConsistentForNSeconds(int seconds) => EndAt = DateTimeOffset.UtcNow.AddSeconds(seconds);
  public bool IsExpired() => !EndAt.IsInFuture();
  public bool IsApplied { get; set; }

}
