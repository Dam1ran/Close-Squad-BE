using CS.Application.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase {
  private readonly ILogger<AuthController> _logger;
  public AuthController(ILogger<AuthController> logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }

  [ProducesResponseType(typeof(IEnumerable<Kkt>), StatusCodes.Status200OK)]
  [ProducesDefaultResponseType]
  [HttpGet("Get")]
  // [HttpPost]
  // [IgnoreAntiforgeryToken]

  public ActionResult<IEnumerable<Kkt>> Get() {
    var ziu = new List<Kkt>();
    ziu.Add(new Kkt { Name = "boris", Cartoafi = new List<Cartof>{ new Cartof { Sort = "okisori" }, new Cartof { Sort = "belicuts" },} });
    ziu.Add(new Kkt { Name = "jake" });
    // var fiu = ziu[3];
    _logger.LogWarning("KIKAT NAHUI");
    return Ok(ziu);
  }

  [HttpGet("Login")]
  [ProducesDefaultResponseType]
  // [ValidateAntiForgeryToken]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  public ActionResult<string> Login() {
    return Ok("pizdet");
  }
}
