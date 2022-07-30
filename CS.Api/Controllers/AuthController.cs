using System.Web;
using CS.Api.Support.Models.Auth;
using CS.Application.Utils;
using CS.Infrastructure.Services;
using CS.Infrastructure.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase {
  private readonly ILogger<AuthController> _logger;
  private readonly IUserService _userService;
  private readonly IOptions<ApiBehaviorOptions> _apiBehaviorOptions;
  public AuthController(ILogger<AuthController> logger, IUserService userService, IOptions<ApiBehaviorOptions> apiBehaviorOptions) {
    _logger = Check.NotNull(logger, nameof(logger));
    _userService = Check.NotNull(userService, nameof(userService));
    _apiBehaviorOptions = Check.NotNull(apiBehaviorOptions, nameof(apiBehaviorOptions));
  }

  [ProducesResponseType(typeof(IEnumerable<Kkt>), StatusCodes.Status200OK)]
  [ProducesDefaultResponseType]
  [HttpGet("Get")]

  public ActionResult<IEnumerable<Kkt>> Get() {
    var ziu = new List<Kkt>();
    ziu.Add(new Kkt { Name = "boris", Cartoafi = new List<Cartof>{ new Cartof { Sort = "okisori" }, new Cartof { Sort = "belicuts" },} });
    ziu.Add(new Kkt { Name = "jake" });
    // var fiu = ziu[3];
    _logger.LogWarning("KIKAT NAHUI");
    return Ok(ziu);
  }

  [AllowAnonymous]
  [HttpGet("Login")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  public async Task<ActionResult<string>> Login() {
    await Task.Run(() => {});
    return Ok("pizdet");
  }

  [AllowAnonymous]
  [HttpPost("register")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto) {
    var identityResult = await _userService.CreateUser(userRegisterDto.Nickname, userRegisterDto.Email, userRegisterDto.Password);
    if (!identityResult.Succeeded) {
      foreach(var err in identityResult.Errors) {
        if (err.Code.Contains("nickname", StringComparison.InvariantCultureIgnoreCase) ||
          err.Code.Contains("name", StringComparison.InvariantCultureIgnoreCase)) {
          ModelState.AddModelError("Nickname", err.Description.Replace($" '{userRegisterDto.Nickname}' ", " ", StringComparison.InvariantCultureIgnoreCase));
        } else if (err.Code.Contains("email", StringComparison.InvariantCultureIgnoreCase)) {
          ModelState.AddModelError("Email", err.Description.Replace($" '{userRegisterDto.Email}' ", " ", StringComparison.InvariantCultureIgnoreCase));
        } else if (err.Code.Contains("password", StringComparison.InvariantCultureIgnoreCase)) {
          ModelState.AddModelError("Password", err.Description);
        } else {
          _logger.LogDebug($"{err.Code} - {err.Description}");
        }
      }

      ModelState.AddModelError("Register", "Register error has occurred.");

      return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

    var sendEmailResponse = await _userService.SendConfirmationEmail(userRegisterDto.Email);
    if (sendEmailResponse.Successful) {
      return Ok("Confirmation email sent to specified address.");
    }

    foreach(var err in sendEmailResponse.Errors) {
      _logger.LogWarning($"Email confirmation error: {err}");
    }

    ModelState.AddModelError("Confirmation", "Error has occurred while sending confirmation email.");
    return _apiBehaviorOptions.Value.InvalidModelStateResponseFactory(ControllerContext);
  }

  [AllowAnonymous]
  [HttpPost("confirm-email")]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<ActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto) {
    

    // var response = await _userService.SendConfirmationEmail(userRegisterDto.Email);

    // if (response.Successful) {
    //   return Ok("Confirmation email sent to specified address");
    // }

    // return BadRequest(response.Errors);
    return NoContent();
  }
}
