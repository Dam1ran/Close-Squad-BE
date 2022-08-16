using System.Net;
using CS.Api.Services.Abstractions;
using CS.Api.Support.Models;
using CS.Api.Support.Models.Abstractions;
using CS.Application.Utils;

namespace CS.Api.Support.Middleware;
public class AuthenticatedClientRateLimiting {
  private readonly RequestDelegate _next;
  private readonly ILogger<AuthenticatedClientRateLimiting> _logger;
  private readonly int TimeWindowInMilliSeconds = 1000;
  private readonly int MaxRequests = 1;

  public AuthenticatedClientRateLimiting(RequestDelegate next, ILogger<AuthenticatedClientRateLimiting> logger) {
    _next = Check.NotNull(next, nameof(next));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext httpContext, IStatisticsCacheService<AuthenticatedClientStatistics> cacheService) {
    var email = GetClientEmail(httpContext);
    if (string.IsNullOrEmpty(email)) {
      await _next(httpContext);
      return;
    }
    var key = $"[{email}]";
    var existingWrapper = await cacheService.GetAsync(key, httpContext.RequestAborted);
    if (existingWrapper?.Entity is not null) {
      if (existingWrapper.Entity.NumberOfRequests >= MaxRequests) {
        httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        return;
      }

      existingWrapper.Entity.NumberOfRequests++;
    }

    await cacheService.SetAsync(
      key,
      existingWrapper ?? StatisticsEntityWrapper<AuthenticatedClientStatistics>.FromEntity(new (1), DateTimeOffset.UtcNow.AddMilliseconds(TimeWindowInMilliSeconds)),
      cancellationToken: httpContext.RequestAborted);

    await _next(httpContext);
  }
  private string GetClientEmail(HttpContext httpContext) {
    return string.Empty; // TODO
  }
}
