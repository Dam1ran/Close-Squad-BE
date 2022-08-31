using CS.Api.Support;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models;
using CS.Api.Support.Models.Auth;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Support;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase {
  private readonly ILogger<AuthController> _logger;
  private readonly IUserManager _userManager;

  private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;
  public AuthController(ILogger<AuthController> logger, IUserManager userManager, IOptions<ApiBehaviorOptions> apiBehaviorOptions) {
    _logger = Check.NotNull(logger, nameof(logger));
    _userManager = Check.NotNull(userManager, nameof(userManager));
    _apiBehaviorOptions = Check.NotNull(apiBehaviorOptions, nameof(apiBehaviorOptions));
  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._20_Minutes_InSeconds, MaxRequests = 4, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPost("register")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto, CancellationToken cancellationToken) {

    var response =
      await _userManager.CreateAsync(
        new Nickname(userRegisterDto.Nickname),
        new Email(userRegisterDto.Email),
        new Password(userRegisterDto.Password),
        cancellationToken);

    if (!response.Successful) {
      foreach(var err in response.ErrorDetails) {
        ModelState.AddModelError(err.Key, err.Value);
      }

      ModelState.AddModelError("Register", "Register error has occurred.");

      return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

    var umResponse = await _userManager.SendConfirmationEmailAsync(new Email(userRegisterDto.Email), cancellationToken);
    if (umResponse.Successful) {
      return Ok("Confirmation email sent to specified address.");
    }

    ModelState.AddModelError("Confirmation", "Error has occurred while sending confirmation email.");

    return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);

  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._20_Minutes_InSeconds, MaxRequests = 4, By = LimitRequestsType.IpAndEndpoint)]
  [SkipCaptchaCheck]
  [HttpPost("confirm-email")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto, CancellationToken cancellationToken) {
    var umResponse = await _userManager.ConfirmEmailAsync(confirmEmailDto.Guid, cancellationToken);
    if (!umResponse.Successful) {
      var (code, description) = umResponse.ErrorDetails.First();
      return BadRequest(new ErrorDetails { Code = code, Description = description });
    }

    return NoContent();
  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._20_Minutes_InSeconds, MaxRequests = 4, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPost("resend-confirmation")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDto resendConfirmationDto, CancellationToken cancellationToken) {
    var umResponse = await _userManager.SendConfirmationEmailAsync(new Email(resendConfirmationDto.Email), cancellationToken);
    if (umResponse.Successful) {
      return Ok("Confirmation email sent to specified address.");
    }

    ModelState.AddModelError("Confirmation", "Error has occurred while sending confirmation email.");

    return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
  }

  [AllowAnonymous]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._15_Minutes_InSeconds, MaxRequests = 5, By = LimitRequestsType.IpAndEndpoint)]
  [HttpPost("login")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto, CancellationToken cancellationToken) {
    var loginResponse = await _userManager
      .LoginAsync(
        new Email(userLoginDto.Email),
        new Password(userLoginDto.Password),
        cancellationToken);

    if (!loginResponse.Successful) {
      var (code, description) = loginResponse.ErrorDetails.First();

      if (loginResponse.IntegerData.HasValue) {
        return BadRequest(new { Code = code, Description = description, data = loginResponse.IntegerData });
      }

      return BadRequest(new ErrorDetails { Code = code, Description = description });
    }

    var cookieOptions = new CookieOptions {
      HttpOnly = true,
      Expires = loginResponse.RefreshToken!.ExpiresAt,
      IsEssential = true,
      Secure = true
    };

    Response.Cookies.Append(Api_Constants.RefreshTokenCookieKey, loginResponse.RefreshToken!.Token, cookieOptions);

    // TODO: middleware will check against that token
    return Ok(new { token = loginResponse.Token, lastLoginInterval = loginResponse.IntegerData });

  }

  // refresh token
  // change password
  // logout

}
