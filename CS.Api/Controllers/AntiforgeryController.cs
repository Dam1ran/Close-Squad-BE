using CS.Api.Support;
using CS.Api.Support.Attributes;
using CS.Application.Support.Utils;
using CS.Core.Support;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[IgnoreAntiforgeryToken]
[Route("[controller]")]
public class AntiforgeryController : ControllerBase {
  private readonly IAntiforgery _antiforgery;

  public AntiforgeryController(IAntiforgery antiforgery) {
    _antiforgery = Check.NotNull(antiforgery, nameof(antiforgery));
  }

  [HttpPost]
  [AllowAnonymous]
  [SkipCaptchaCheck]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._20_Minutes_InSeconds, MaxRequests = 4, By = LimitRequestsType.IpAndEndpoint)]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult GenerateAntiforgeryToken() {
    var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
    if (tokens.RequestToken is null) {
      return BadRequest("No request token");
    }

    Response.Cookies.Append(
      Api_Constants.AntiforgeryCookiePlaceholder,
      tokens.RequestToken,
      new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.None });

    return NoContent();
  }

}
