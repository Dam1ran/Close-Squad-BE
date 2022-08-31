using System.Text.RegularExpressions;
using CS.Core.Exceptions;

namespace CS.Core.ValueObjects;
public class Password : ValuesObject {
  protected Password() {}
  public Password(string password) {
    Match match = (new Regex(RegexPattern)).Match(password);
    if (
      !match.Success ||
      match.Index != 0 ||
      match.Length != password.Length ||
      password.Length < MinLength ||
      password.Length > MaxLength)
    {
      throw new DomainValidationException($"{nameof(password)} does not match regex.");
    }

    Value = password;
  }
  public const int MinLength = 8;
  public const int MaxLength = 64;
  public const string RegexPattern =
    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$";

  public string Value { get; private set; } = string.Empty;

  public override string ToString() => Value;

  protected override IEnumerable<object> GetEqualityComponents() {
    yield return Value;
  }

}
