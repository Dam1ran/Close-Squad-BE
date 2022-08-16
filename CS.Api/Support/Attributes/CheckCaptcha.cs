namespace CS.Api.Support.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class CheckCaptcha : Attribute {
  public const int DefaultMaxAllowedAttempts = 4;
  public int MaxAllowedAttempts { get; set; } = DefaultMaxAllowedAttempts;
}
