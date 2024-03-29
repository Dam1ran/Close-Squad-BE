using System.Security.Claims;
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

namespace CS.Api.Controllers.Auth;
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
  [IgnoreAntiforgeryToken]
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
  [IgnoreAntiforgeryToken]
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
  [IgnoreAntiforgeryToken]
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
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._15_Minutes_InSeconds, MaxRequests = 5, By = LimitRequestsType.IpAndEndpoint)]
  [HttpPost("login")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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
      SameSite = SameSiteMode.None,
      HttpOnly = true,
      Expires = loginResponse.RefreshToken!.ExpiresAt,
      IsEssential = true,
      Secure = true
    };

    Response.Cookies.Append(Api_Constants.RefreshTokenCookieKey, loginResponse.RefreshToken!.Token, cookieOptions);

    return Ok(
      new {
        token = loginResponse.Token,
        lastLoginInterval = loginResponse.IntegerData,
        sessionId = loginResponse.Guid
      });

  }

  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._30_Minutes_InSeconds, MaxRequests = 3, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPost("send-change-password")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SendChangePasswordEmail([FromBody] ChangePasswordEmailDto changePasswordDto, CancellationToken cancellationToken) {
    var response = await _userManager
      .SendChangePasswordEmailAsync(
        new Email(changePasswordDto.Email),
        cancellationToken);

    if (!response.Successful) {

      var (code, description) = response.ErrorDetails.First();
      return BadRequest(new ErrorDetails { Code = code, Description = description });

    }

    return Ok("Change password email sent to specified address.");

  }

  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._30_Minutes_InSeconds, MaxRequests = 3, By = LimitRequestsType.IpAndEndpoint)]
  [CheckCaptcha]
  [HttpPost("change-password")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto, CancellationToken cancellationToken) {
    var response = await _userManager
      .ChangePasswordAsync(
        changePasswordDto.Guid,
        new Password(changePasswordDto.Password),
        cancellationToken);

    if (!response.Successful) {

      var (code, description) = response.ErrorDetails.First();
      return BadRequest(new ErrorDetails { Code = code, Description = description });

    }

    return NoContent();

  }

  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.IpAndEndpoint)]
  [HttpPost("refresh-token")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public async Task<IActionResult> RefreshToken([FromBody] SessionIdDto sessionIdDto, CancellationToken cancellationToken) {

    var refreshToken = Request.Cookies[Api_Constants.RefreshTokenCookieKey];
    if (string.IsNullOrWhiteSpace(refreshToken)) {
      return Unauthorized();
    }

    var result = await _userManager.RefreshTokenAsync(sessionIdDto.SessionId, refreshToken, cancellationToken);
    if (!result.Successful) {
      return Unauthorized();
    }

    return Ok(result.Token);

  }

  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._15_Minutes_InSeconds, MaxRequests = 6, By = LimitRequestsType.IpAndEndpoint)]
  [HttpPost("logout")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]

  public async Task<IActionResult> Logout(CancellationToken cancellationToken) {
    var nickname = (User.Identity as ClaimsIdentity)!.FindFirst("nickname")!.Value;

    await _userManager.LogoutAsync(new Nickname(nickname), cancellationToken);

    foreach(var cookie in Request.Cookies) {
      Response.Cookies.Append(
        cookie.Key, "",
        new CookieOptions {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.None,
          Expires = DateTimeOffset.MinValue
        });
    }

    return NoContent();
  }

}
