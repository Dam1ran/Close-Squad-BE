using System.Text.RegularExpressions;
using CS.Core.Exceptions;

namespace CS.Core.ValueObjects;
public class Nickname : ValuesObject {
  protected Nickname() {}
  public Nickname(string nicknameValue) {
    if (IsWrongNickname(nicknameValue)) {
      throw new DomainValidationException($"{nameof(nicknameValue)} does not match regex.");
    }

    Value = nicknameValue;
    ValueLowerCase = nicknameValue.ToLowerInvariant();
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

  public static bool IsWrongNickname(string nicknameValue, out Nickname? nickname) {
    if (IsWrongNickname(nicknameValue)) {
      nickname = null;
      return true;
    }

    nickname = new Nickname(nicknameValue);
    return false;

  }

  public static bool IsWrongNickname(string nicknameValue) {
    Match match = (new Regex(RegexPattern)).Match(nicknameValue);

    return
      !match.Success ||
      match.Index != 0 ||
      match.Length != nicknameValue.Length ||
      nicknameValue.Length < MinLength ||
      nicknameValue.Length > MaxLength;

  }

}
