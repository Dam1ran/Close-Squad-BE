using System.Text.RegularExpressions;
using CS.Core.Exceptions;

namespace CS.Core.ValueObjects;
public class Nickname : ValuesObject {
  protected Nickname() {}
  public Nickname(string nickname) {
    Match match = (new Regex(RegexPattern)).Match(nickname);
    if (
      !match.Success ||
      match.Index != 0 ||
      match.Length != nickname.Length ||
      nickname.Length < MinLength ||
      nickname.Length > MaxLength)
    {
      throw new DomainValidationException($"{nameof(nickname)} does not match regex.");
    }

    Value = nickname;
    ValueLowerCase = nickname.ToLowerInvariant();
  }
  public const int MinLength = 4;
  public const int MaxLength = 20;
  public const string RegexPattern = @"^[A-Za-z0-9]+([_](?!$))?[A-Za-z0-9]*$";
  public string Value { get; private set; } = string.Empty;
  public string ValueLowerCase { get; private set; } = string.Empty;

  public override string ToString() => Value;

  protected override IEnumerable<object> GetEqualityComponents() {
    yield return Value;
  }

}
