using System.Text.RegularExpressions;
using CS.Core.Exceptions;

namespace CS.Core.ValueObjects;
public class Email : ValuesObject {
  protected Email() {}
  public Email(string email) {
    Match match = (new Regex(RegexPattern)).Match(email);
    if (
      !match.Success ||
      match.Index != 0 ||
      match.Length != email.Length ||
      email.Length < MinLength ||
      email.Length > MaxLength)
    {
      throw new DomainValidationException($"{nameof(email)} does not match regex.");
    }

    Value = email;
    ValueLowerCase = email.ToLowerInvariant();
  }

  public const int MinLength = 5;
  public const int MaxLength = 255;
  public const string RegexPattern = @"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$";
  public string Value { get; private set; } = string.Empty;
  public string ValueLowerCase { get; private set; } = string.Empty;

  public override string ToString() => Value;

  protected override IEnumerable<object> GetEqualityComponents() {
    yield return Value;
  }

}
