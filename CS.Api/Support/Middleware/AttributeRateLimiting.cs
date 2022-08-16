using System.Net;
using System.Security.Claims;
using CS.Api.Services.Abstractions;
using CS.Api.Support.Attributes;
using CS.Api.Support.Exceptions;
using CS.Api.Support.Models;
using CS.Api.Support.Models.Abstractions;
using CS.Application.Utils;

namespace CS.Api.Support.Middleware;
public class AttributeRateLimiting {
  private readonly RequestDelegate _next;
  private readonly ILogger<AttributeRateLimiting> _logger;

  public AttributeRateLimiting(RequestDelegate next, ILogger<AttributeRateLimiting> logger) {
    _next = Check.NotNull(next, nameof(next));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext httpContext, IStatisticsCacheService<ClientStatistics> cacheService) {
    Check.NotNull(httpContext, nameof(httpContext));
    Check.NotNull(cacheService, nameof(cacheService));

    var limitRequests = httpContext.GetEndpoint()?.Metadata.GetMetadata<LimitRequests>();
    if (limitRequests is null || IsGameMaster(httpContext)) {
      await _next(httpContext);
      return;
    }

    var key = string.Empty;
    switch (limitRequests.By) {
      case LimitRequestsType.Endpoint : {
        key = GetEndpoint(httpContext);
        break;
      }
      case LimitRequestsType.IpAndEndpoint : {
        if (IsClientIpMissing(httpContext, out string clientIp)) {
          _logger.LogWarning("Created empty statistics IP key.");
          httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
          return;
        }
        _logger.LogInformation($"Created statistics IP key: [{clientIp}].");
        key = ComposeClientStatisticsKey(clientIp, GetEndpoint(httpContext));
        break;
      }
      case LimitRequestsType.RoleAndEndpoint : {
        var clientRole = GetClientRole(httpContext);
        if (string.IsNullOrEmpty(clientRole)) {
          httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
          return;
        }
        key = ComposeClientStatisticsKey(clientRole, GetEndpoint(httpContext));
        break;
      }
      case LimitRequestsType.EmailCredentialAndEndpoint : {
        var clientEmail = GetClientEmail(httpContext);
        if (string.IsNullOrEmpty(clientEmail)) {
          httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
          return;
        }
        key = ComposeClientStatisticsKey(clientEmail, GetEndpoint(httpContext));
        break;
      }
      default: throw new MiddlewareException(nameof(AttributeRateLimiting), $"Case not defined for {nameof(LimitRequestsType)}.{limitRequests.By}.");
    }

    var existingWrapper = await cacheService.GetAsync(key, httpContext.RequestAborted);
    if (existingWrapper?.Entity is not null) {
      if (existingWrapper.Entity.NumberOfRequests >= limitRequests.MaxRequests) {
        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        return;
      }

      existingWrapper.Entity.NumberOfRequests++;
    }

    await cacheService.SetAsync(
      key,
      existingWrapper ?? StatisticsEntityWrapper<ClientStatistics>.FromEntity(new (1), DateTimeOffset.UtcNow.AddSeconds(limitRequests.TimeWindowInSeconds)),
      cancellationToken: httpContext.RequestAborted);

    await _next(httpContext);
  }

  private bool IsGameMaster(HttpContext httpContext) {
    return false; // TODO
  }

  private string GetEndpoint(HttpContext httpContext) => $"{httpContext.Request.Path}";
  private bool IsClientIpMissing(HttpContext httpContext, out string ip) {
    ip = $"{httpContext.Connection.RemoteIpAddress}";
    return string.IsNullOrWhiteSpace(ip);
  }
  private string GetClientRole(HttpContext httpContext) {
    return string.Empty; // TODO
  }
  private string GetClientEmail(HttpContext httpContext) {
    // var typeKey = string.Empty;
    // if (httpContext.User.Identity is ClaimsIdentity identity) {
    //   email = identity.FindFirst(ClaimTypes.Email).Value;
    // }

    return string.Empty; // TODO
  }

  private string ComposeClientStatisticsKey(string firstKey, string secondKey) => $"[{firstKey}]: {secondKey}";

}
