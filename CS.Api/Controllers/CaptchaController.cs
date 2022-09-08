using CS.Api.Support;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
[IgnoreAntiforgeryToken]
public class CaptchaController : ControllerBase {

  private readonly ICaptchaService _captchaService;
  private readonly ICacheService _cacheService;

  public CaptchaController(ICaptchaService captchaService, ICacheService cacheService) {
    _captchaService = Check.NotNull(captchaService, nameof(captchaService));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  [AllowAnonymous]
  [SkipCaptchaCheck]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._10_Minutes_InSeconds, MaxRequests = 15, By = LimitRequestsType.IpAndEndpoint)]
  [HttpGet]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
  public async Task<ActionResult<string>> Get(CancellationToken cancellationToken) {
    var guid = Guid.NewGuid().ToString();
    var keycode = _captchaService.GetCode();

    var image = _captchaService.GetImage(keycode);

    await _cacheService.SetAsync(
      CacheGroupKeyConstants.Captcha,
      guid,
      new CachedCaptcha { Keycode = keycode },
      absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(1),
      cancellationToken: cancellationToken);
    HttpContext.Session.SetString(Api_Constants.ClientIdKey, guid);

    return File(image.Stream, image.ContentType);
  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._10_Minutes_InSeconds, MaxRequests = 10, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPatch]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public IActionResult Ping() => NoContent();

}
