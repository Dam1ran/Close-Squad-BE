namespace CS.Core.ValueObjects;
public abstract class ValuesObject {
  protected abstract IEnumerable<object> GetEqualityComponents();

  public override bool Equals(object? obj) {
    if (obj == null || obj.GetType() != GetType()) {
      return false;
    }

    var target = (ValuesObject)obj;

    return GetEqualityComponents().SequenceEqual(target.GetEqualityComponents());
  }

  public override int GetHashCode() => GetEqualityComponents().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);

  public ValuesObject GetCopy() => (MemberwiseClone() as ValuesObject)!;

  public static bool operator ==(ValuesObject left, ValuesObject right) {
    if (Equals(left, null)) {
      return Equals(right, null);
    } else {
      return left.Equals(right);
    }
  }

  public static bool operator !=(ValuesObject left, ValuesObject right) => !(left == right);

}
