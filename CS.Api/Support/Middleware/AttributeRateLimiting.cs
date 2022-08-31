using System.Net;
using CS.Api.Support.Attributes;
using CS.Api.Support.Exceptions;
using CS.Api.Support.Models.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;

namespace CS.Api.Support.Middleware;
public class AttributeRateLimiting {
  private readonly RequestDelegate _next;
  private readonly ICacheService _cacheService;
  private readonly ILogger<AttributeRateLimiting> _logger;

  public AttributeRateLimiting(
    RequestDelegate next,
    ICacheService cacheService,
    ILogger<AttributeRateLimiting> logger) {
    _next = Check.NotNull(next, nameof(next));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext httpContext) {
    Check.NotNull(httpContext, nameof(httpContext));

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
      case LimitRequestsType.NicknameCredentialAndEndpoint : {
        var nickname = GetClientNickname(httpContext);
        if (string.IsNullOrEmpty(nickname)) {
          httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
          return;
        }
        key = ComposeClientStatisticsKey(nickname, GetEndpoint(httpContext));
        break;
      }
      default: throw new MiddlewareException(nameof(AttributeRateLimiting), $"Case not defined for {nameof(LimitRequestsType)}.{limitRequests.By}.");
    }

    var statistics = await _cacheService.GetAsync<StatisticsEntity>(CacheGroupKeyConstants.AttributeRateLimiting, key, httpContext.RequestAborted);
    if (statistics is not null) {
      if (statistics.NumberOfRequests >= limitRequests.MaxRequests) {
        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        return;
      }

      statistics.NumberOfRequests++;
    }

    var expiresAt = DateTimeOffset.UtcNow.AddSeconds(limitRequests.TimeWindowInSeconds);
    var saveStatistics = statistics ?? new StatisticsEntity(1, expiresAt);
    await _cacheService.SetAsync(
      CacheGroupKeyConstants.AttributeRateLimiting,
      key,
      saveStatistics,
      absoluteExpiration: saveStatistics.ExpiresAt,
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
  private string GetClientNickname(HttpContext httpContext) {
    // var typeKey = string.Empty;
    // if (httpContext.User.Identity is ClaimsIdentity identity) {
    //   email = identity.FindFirst(ClaimTypes.Email).Value;
    // }

    return string.Empty; // TODO
  }

  private string ComposeClientStatisticsKey(string firstKey, string secondKey) => $"[{firstKey}]: {secondKey}";

}
