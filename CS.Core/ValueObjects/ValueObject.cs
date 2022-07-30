namespace CS.Core.ValueObjects;
public abstract class ValueObject {
  protected abstract IEnumerable<object> GetEqualityComponents();

  public override bool Equals(object? obj) {
    if (obj == null || obj.GetType() != GetType()) {
      return false;
    }

    var target = (ValueObject)obj;

    return GetEqualityComponents().SequenceEqual(target.GetEqualityComponents());
  }

  public override int GetHashCode() => GetEqualityComponents().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);

  public ValueObject GetCopy() => (MemberwiseClone() as ValueObject)!;

  public static bool operator ==(ValueObject left, ValueObject right) {
    if (Equals(left, null)) {
      return Equals(right, null);
    } else {
      return left.Equals(right);
    }
  }

  public static bool operator !=(ValueObject left, ValueObject right) => !(left == right);
}
