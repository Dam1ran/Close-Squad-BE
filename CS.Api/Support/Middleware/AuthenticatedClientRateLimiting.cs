using System.Net;
using System.Security.Claims;
using CS.Api.Support.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using Microsoft.AspNetCore.Authorization;

namespace CS.Api.Support.Middleware;
public class AuthenticatedClientRateLimiting {
  private readonly RequestDelegate _next;
  private readonly ICacheService _cacheService;
  private readonly ILogger<AuthenticatedClientRateLimiting> _logger;
  private readonly int TimeWindowInMilliSeconds = 1000;
  private readonly int MaxRequests = 2;

  public AuthenticatedClientRateLimiting(
    RequestDelegate next,
    ICacheService cacheService,
    ILogger<AuthenticatedClientRateLimiting> logger) {
    _next = Check.NotNull(next, nameof(next));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext httpContext) {
    var allowAnonymousAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<AllowAnonymousAttribute>();
    if (allowAnonymousAttribute is not null) {
      await _next(httpContext);
      return;
    }

    var nickname = GetClientNickname(httpContext);
    if (string.IsNullOrEmpty(nickname)) {
      await _next(httpContext);
      return;
    }

    var key = $"[{nickname}]";
    var statistics = await _cacheService
      .GetAsync<StatisticsEntity>(CacheGroupKeyConstants.AuthenticatedClientStatistics, key, httpContext.RequestAborted);

    if (statistics is not null) {
      if (statistics.NumberOfRequests >= MaxRequests) {
        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "UserThrottle", Description = "User actions too frequent." });
        return;
      }

      statistics.NumberOfRequests++;
    }

    var expiresAt = DateTimeOffset.UtcNow.AddMilliseconds(TimeWindowInMilliSeconds);
    var saveStatistics = statistics ?? new StatisticsEntity(1, expiresAt);
    await _cacheService.SetAsync(
      CacheGroupKeyConstants.AuthenticatedClientStatistics,
      key,
      saveStatistics,
      absoluteExpiration: saveStatistics.ExpiresAt,
      cancellationToken: httpContext.RequestAborted);

    await _next(httpContext);
  }
  private string GetClientNickname(HttpContext httpContext) {

    var nickname = string.Empty;

    if (httpContext.User.Identity is ClaimsIdentity identity) {
      nickname = identity.FindFirst("nickname")?.Value ?? string.Empty;
    }

    return nickname;

  }

}
