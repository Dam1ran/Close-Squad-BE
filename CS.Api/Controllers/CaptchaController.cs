using System.Diagnostics;
using CS.Api.Services.Abstractions;
using CS.Api.Support;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models;
using CS.Application.Utils;
using CS.Core.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class CaptchaController : ControllerBase {

  private readonly ICaptchaService _captchaService;
  private readonly ICaptchaCacheService _captchaCacheService;

  public CaptchaController(ICaptchaService captchaService, ICaptchaCacheService captchaCacheService) {
    _captchaService = Check.NotNull(captchaService, nameof(captchaService));
    _captchaCacheService = Check.NotNull(captchaCacheService, nameof(captchaCacheService));
  }

  [AllowAnonymous]
  [SkipCaptchaCheck]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._10_Minutes_InSeconds, MaxRequests = 5, By = LimitRequestsType.IpAndEndpoint)]
  [HttpGet]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
  public async Task<ActionResult<string>> Get(CancellationToken cancellationToken) {
    var guid = Guid.NewGuid().ToString();
    var keycode = _captchaService.GetCode(); Debug.WriteLine(keycode);

    var image = _captchaService.GetImage(keycode);

    await _captchaCacheService.SetAsync(guid, new CachedCaptcha { Keycode = keycode }, cancellationToken);
    HttpContext.Session.SetString(Api_Constants.ClientIdKey, guid);

    return File(image.Stream, image.ContentType);
  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._10_Minutes_InSeconds, MaxRequests = 5, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPatch]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public IActionResult Ping() => NoContent();
}