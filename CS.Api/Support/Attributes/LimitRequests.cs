namespace CS.Api.Support.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class LimitRequests : Attribute {
  public int TimeWindowInSeconds { get; set; } = 600;
  public int MaxRequests { get; set; } = 2;
  public LimitRequestsType By { get; set; } = LimitRequestsType.Endpoint;
}

public enum LimitRequestsType {
  Endpoint = 0,
  IpAndEndpoint = 1,
  RoleAndEndpoint = 2,
  EmailCredentialAndEndpoint = 4,
}
