namespace CS.Core.Extensions;
public static class DateTimeOffsetExtensions {
  public static bool IsInFuture(this DateTimeOffset value)
    => DateTimeOffset.Compare(value, DateTimeOffset.UtcNow) > 0;

}
