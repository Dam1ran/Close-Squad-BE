using CS.Core.Exceptions;

namespace CS.Core.ValueObjects;
public class Name : ValueObject {
  protected Name() {}
  public Name(string name) {
    if (string.IsNullOrWhiteSpace(name)) {
      throw new DomainValidationException(nameof(name));
    }

    Value = name;
  }
  public const int MaxLength = 20;
  public string Value { get; private set; } = string.Empty;

  public override string ToString() => Value;

  protected override IEnumerable<object> GetEqualityComponents() {
    yield return Value;
  }
}
