using CS.Core.Entities.Abstractions;
using CS.Core.Exceptions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities.Auth;
public class Verification : Entity {
  #nullable disable
  protected Verification() { }
  #nullable restore
  public Verification(Email email) {
    Email = email ?? throw new DomainValidationException(nameof(email));
  }

  public Email Email { get; private set; }
  public bool EmailConfirmed { get; private set; } = false;
  public bool LockoutEmailSent { get; set; } = false;
  public DateTimeOffset LastLoginAt { get; private set; } = DateTimeOffset.UtcNow;
  public bool CheckCaptcha { get; private set; } = false;
  public bool Banned { get; private set; } = false;
  public string BanReason { get; private set; } = string.Empty;

  public void EnableCheckCaptcha() => CheckCaptcha = true;
  public void DisableCheckCaptcha() => CheckCaptcha = false;
  public void SetEmailConfirmed() => EmailConfirmed = true;
  public void UnsetEmailConfirmed() => EmailConfirmed = false;
  public void Ban(string reason) {
    if (string.IsNullOrWhiteSpace(reason)) {
      throw new DomainValidationException($"{nameof(reason)} cannot be empty for ban reason.");
    }

    BanReason = reason;
    Banned = true;
  }

  public void Unban() => Banned = false;

  /// <summary>
  /// Sets login time to DateTimeOffset.UtcNow.
  /// </summary>
  /// <returns>TimeSpan difference between previous and current method call.</returns>
  public TimeSpan SetLoginDateTimeAndGetInterval() {
    var difference = LastLoginAt - DateTimeOffset.UtcNow;
    LastLoginAt = DateTimeOffset.UtcNow;
    return difference;
  }

}
