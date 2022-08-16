namespace CS.Core.Entities.Abstractions;
public abstract class Entity {
  private int? _hashCode;
  private long _Id;

  public virtual long Id {
    get => _Id;
    protected set => _Id = value;
  }

  public bool IsTransient => Id == default;

  public override bool Equals(object? obj) {
    if (obj == null || !(obj is Entity)) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (GetType() != obj.GetType()) {
      return false;
    }

    var item = (Entity)obj;
    if (item.IsTransient || IsTransient) {
      return false;
    }

    return item.Id == Id;
  }

  public override int GetHashCode() {

    if (!IsTransient) {
      if (!_hashCode.HasValue) {
        _hashCode = Id.GetHashCode() ^ 31;
      }

      return _hashCode.Value;
    }

    return base.GetHashCode();
  }

  public static bool operator ==(Entity left, Entity right) {

    if (Equals(left, null)) {
      return Equals(right, null);
    }

    return left.Equals(right);
  }

  public static bool operator !=(Entity left, Entity right) => !(left == right);
}
