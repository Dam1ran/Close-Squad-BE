using System.Security.Cryptography;
using System.Text;
using CS.Core.Entities.Abstractions;
using CS.Core.Extensions;
using CS.Core.ValueObjects;

namespace CS.Core.Entities.Auth;
public class IdentificationPassword : Entity {
  #nullable disable
  protected IdentificationPassword() { }
  #nullable restore
  public const int MaxAllowedAttempts = 4;
  public const int LockoutMinutes = 30;
  public IdentificationPassword(Password password) {

    using var hmac = new HMACSHA512();

    Salt = hmac.Key;
    Hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password.Value));

  }

  public byte[] Salt { get; private set; }
  public byte[] Hash { get; private set; }
  public int AccessAttempts { get; private set; }
  public DateTimeOffset LockoutEndsAt { get; private set; } = DateTimeOffset.UtcNow;

  public bool CheckPasswordAndRecordAttempt(Password password) {
    using var hmac = new HMACSHA512(Salt);

    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password.Value));
    var isEqual = computedHash.SequenceEqual(Hash);
    if (!isEqual) {
      RecordAttempt();
    }

    return isEqual;
  }

  public void RecordAttempt() {
    AccessAttempts++;
    if (AccessAttempts > MaxAllowedAttempts) {
      LockoutEndsAt = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);
    }
  }

  public void ResetAttempts() {
    AccessAttempts = default;
    LockoutEndsAt = DateTimeOffset.UtcNow;
  }

  public bool IsLockedOut() => LockoutEndsAt.IsInFuture();

}
