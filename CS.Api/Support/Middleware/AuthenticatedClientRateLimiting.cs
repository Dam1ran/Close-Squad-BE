using System.Net;
using CS.Api.Support.Models.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;

namespace CS.Api.Support.Middleware;
public class AuthenticatedClientRateLimiting {
  private readonly RequestDelegate _next;
  private readonly ICacheService _cacheService;
  private readonly ILogger<AuthenticatedClientRateLimiting> _logger;
  private readonly int TimeWindowInMilliSeconds = 1000;
  private readonly int MaxRequests = 1;

  public AuthenticatedClientRateLimiting(
    RequestDelegate next,
    ICacheService cacheService,
    ILogger<AuthenticatedClientRateLimiting> logger) {
    _next = Check.NotNull(next, nameof(next));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext httpContext) {

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
    return string.Empty; // TODO
  }

}
