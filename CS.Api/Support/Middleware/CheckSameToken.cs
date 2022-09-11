
using System.Net;
using System.Security.Claims;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;

namespace CS.Api.Support.Middleware;
public class CheckSameToken {

  private readonly RequestDelegate _next;
  private readonly ICacheService _cacheService;

  public CheckSameToken(
    RequestDelegate next,
    ICacheService cacheService) {
    _next = Check.NotNull(next, nameof(next));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task InvokeAsync(HttpContext httpContext) {
    Check.NotNull(httpContext, nameof(httpContext));

    var token = httpContext.Request.Headers[Api_Constants.AuthorizationHeader].FirstOrDefault()?.Split(" ").Last();
    if (string.IsNullOrWhiteSpace(token)) {
      await _next(httpContext);
      return;
    }

    var cachedToken = await _cacheService
      .GetStringAsync(CacheGroupKeyConstants.UserJwt, GetClaim(httpContext, "nickname"), httpContext.RequestAborted);

    if (string.IsNullOrEmpty(cachedToken) ||!cachedToken.SequenceEqual(GetClaim(httpContext, "jti"))) {
      httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
      return;
    }

    await _next(httpContext);
  }

  private string GetClaim(HttpContext httpContext, string claimName) {
    var claim = string.Empty;
    if (httpContext.User.Identity is ClaimsIdentity identity) {
      claim = identity.FindFirst(claimName)?.Value ?? string.Empty;
    }
    return claim;
  }

}
