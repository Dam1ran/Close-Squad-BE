using CS.Api.Support;
using CS.Application.Utils;
using Microsoft.AspNetCore.Antiforgery;
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

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public IActionResult GenerateAntiforgeryTokens() {
    var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
    if (tokens.RequestToken is null) {
      return BadRequest("No request token");
    }

    Response.Cookies.Append(
      ApiConstants.AntiforgeryCookiePlaceholder,
      tokens.RequestToken,
      new CookieOptions { HttpOnly = false, Secure = true, SameSite = SameSiteMode.None, Expires = DateTimeOffset.Now.AddDays(1) });

    return NoContent();
  }
}
