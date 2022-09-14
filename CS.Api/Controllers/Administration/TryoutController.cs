using CS.Api.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers.Administration;
[ApiController]
[Route("[controller]")]
public class TryoutController : ControllerBase {

  public TryoutController() {}

  [HttpGet("test1")]
  [Authorize(Policy = Api_Constants.ManagementPolicy)]

  public IActionResult Test1() {
    

    return NoContent();
  }

  [HttpGet("test2")]
  [Authorize(Policy = Api_Constants.AdministrationPolicy)]

  public IActionResult Test2() {
    

    return NoContent();
  }
  [HttpGet("test3")]
  [Authorize(Policy = Api_Constants.GameMasterPolicy)]

  public IActionResult Test3() {
    

    return NoContent();
  }

  [HttpGet("test4")]
  public IActionResult Test4() {
    

    return NoContent();
  }
  [HttpGet("test5")]
  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  public IActionResult Test5() {
    

    return Ok("dnishe ebannaea");
  }

}
