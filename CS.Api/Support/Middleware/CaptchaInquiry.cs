using System.Net;
using CS.Api.Services.Abstractions;
using CS.Api.Support.Attributes;
using CS.Api.Support.Exceptions;
using CS.Api.Support.Models;
using CS.Application.Extensions;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.ValueObjects;

namespace CS.Api.Support.Middleware;
public class CaptchaInquiry {
  private readonly RequestDelegate _next;
  private readonly ILogger<CaptchaInquiry> _logger;
  private readonly ICachedUserService _cachedUserService;
  private readonly ICacheService _cacheService;

  public CaptchaInquiry(
    RequestDelegate next,
    ILogger<CaptchaInquiry> logger,
    ICachedUserService cachedUserService,
    ICacheService cacheService) {
    _next = Check.NotNull(next, nameof(next));
    _logger = Check.NotNull(logger, nameof(logger));
    _cachedUserService = Check.NotNull(cachedUserService, nameof(cachedUserService));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task InvokeAsync(HttpContext httpContext) {
    var skipCaptchaAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<SkipCaptchaCheck>();
    if (skipCaptchaAttribute is not null) {
      await _next(httpContext);
      return;
    }

    var checkCaptchaAttribute = httpContext.GetEndpoint()?.Metadata.GetMetadata<CheckCaptcha>();
    var isNicknameMissing = IsNicknameMissing(httpContext, out string nickname);
    var isCaptchaAttributeNull = checkCaptchaAttribute is null;
    if (isNicknameMissing && isCaptchaAttributeNull) {
      await _next(httpContext);
      return;
    }

    CachedUser? cachedUser = null;
    if (!isNicknameMissing) {
      cachedUser = await _cachedUserService.GetByNicknameAsync(new Nickname(nickname), httpContext.RequestAborted);
    }

    var userHasToCheckCaptcha = IsCheckUserCaptcha(cachedUser);
    if (!userHasToCheckCaptcha && isCaptchaAttributeNull) {
      await _next(httpContext);
      return;
    }

    var captchaCodeMissing = IsCaptchaCodeMissing(httpContext, out string captchaCode);
    if (captchaCodeMissing && userHasToCheckCaptcha) {
      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "CaptchaCheck", Description = "User is assigned to a captcha check." });
      return;
    } else if (captchaCodeMissing && !userHasToCheckCaptcha) {
      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "MissingCaptchaCode", Description = "Captcha code is missing in the request." });
      return;
    }

    var clientIdGuid = httpContext.Session.GetString(Api_Constants.ClientIdKey);
    if (string.IsNullOrWhiteSpace(clientIdGuid)) {
      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "MissingSessionClientId", Description = "Cannot check captcha for missing session client Id." });
      return;
    }

    var cachedCaptcha = await _cacheService.GetAsync<CachedCaptcha>(CacheGroupKeyConstants.Captcha, clientIdGuid, httpContext.RequestAborted);
    if (cachedCaptcha is null) {
      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "CaptchaExpired", Description = "Captcha has been expired." });
      return;
    }

    if (cachedCaptcha.Attempts >= (checkCaptchaAttribute?.MaxAllowedAttempts ?? CheckCaptcha.DefaultMaxAllowedAttempts)) {
      await SetCaptcha(clientIdGuid, cachedCaptcha, httpContext);

      if (IsClientIpMissing(httpContext, out string clientIp))
      {
        _logger.LogWarning("Created empty captcha check IP key.");
      }
      var description = "Too many attempts to validate same captcha.";
      _logger.LogWarning($"[{clientIp}] [{httpContext.Request.Path}] {description}");
      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "TooManyAttempts", Description = description });
      return;
    }

    // if (!captchaCode.Equals(cachedCaptcha.Keycode, StringComparison.InvariantCultureIgnoreCase)) {
    if (!captchaCode.EqualsExceptNCharacters(cachedCaptcha.Keycode, 1)) {
      cachedCaptcha.Attempts++;
      await SetCaptcha(clientIdGuid, cachedCaptcha, httpContext);

      httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
      await httpContext.Response.WriteAsJsonAsync(new ErrorDetails { Code = "WrongCaptcha", Description = "Captcha codes does not match." });
      return;
    }

    if (httpContext.Request.Method != HttpMethod.Patch.Method || !"/captcha".Equals(httpContext.Request.Path, StringComparison.InvariantCultureIgnoreCase)) {
      httpContext.Session.Remove(Api_Constants.ClientIdKey);
      await _cacheService.RemoveAsync(CacheGroupKeyConstants.Captcha, clientIdGuid, httpContext.RequestAborted);

      if (isCaptchaAttributeNull && cachedUser is not null) {
        cachedUser.CheckCaptcha = false;
        var updateResult = await _cachedUserService.UpdateAsync(new Nickname(nickname), cachedUser, httpContext.RequestAborted);
        if (!updateResult.Successful) {
          foreach (var error in updateResult.ErrorDetails) {
            throw new MiddlewareException(nameof(CaptchaInquiry), $"Code: [{error.Key}] Description: {error.Value}");
          }
        }
      }
    }


    await _next(httpContext);
  }

  private async Task SetCaptcha(string clientIdGuid, CachedCaptcha cachedCaptcha, HttpContext httpContext) =>
    await _cacheService
      .SetAsync(
        CacheGroupKeyConstants.Captcha,
        clientIdGuid,
        cachedCaptcha,
        absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(1),
        cancellationToken: httpContext.RequestAborted);


  private bool IsCheckUserCaptcha(CachedUser? cachedUser) {
    return cachedUser?.CheckCaptcha ?? false;
  }
  private bool IsNicknameMissing(HttpContext httpContext, out string nickname) {
    nickname = string.Empty; // TODO
    nickname = "Mime"; // TODO
    // if (httpContext.User.Identity is ClaimsIdentity identity) {
    //   // nickname = identity.FindFirst(ClaimTypes.Email)!.Value;
    // }
    return string.IsNullOrWhiteSpace(nickname);
  }

  private bool IsCaptchaCodeMissing(HttpContext httpContext, out string captchaCode) {
    captchaCode = httpContext.Request.Query["captcha"].ToString().Split(",")[0];
    return string.IsNullOrWhiteSpace(captchaCode);
  }
  private bool IsClientIpMissing(HttpContext httpContext, out string ip) {
    ip = $"{httpContext.Connection.RemoteIpAddress}";
    return string.IsNullOrWhiteSpace(ip);
  }

}
