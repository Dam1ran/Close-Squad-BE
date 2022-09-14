namespace CS.Api.Support;
public static class Api_Constants {

  public const string AntiforgeryHeaderPlaceholder = "xsrf-token";
  public const string AntiforgeryCookiePlaceholder = "x-xsrf-token";
  public const string ContentType = "content-type";
  public const string ClientIdKey = "ClientId";
  public const string RefreshTokenCookieKey = "x-refresh-token";
  public const string AuthorizationHeader = "Authorization";
  public const string ManagementPolicy = "RequireAdmOrGmaRole";
  public const string AdministrationPolicy = "RequireAdmRole";
  public const string GameMasterPolicy = "RequireGmaRole";

}
