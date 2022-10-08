using CS.Api.Support.Attributes;
using CS.Application.Models;
using CS.Application.Support.Utils;
using CS.Core.Services.Interfaces;
using CS.Core.Support;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class SettingsController : ControllerBase {

  private readonly IWorldMapService _worldMapService;

  public SettingsController(IWorldMapService worldMapService) {
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
  }

  [HttpGet("game")]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.IpAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(GameSettings), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public ActionResult Get(CancellationToken cancellationToken) {

    var settings = new GameSettings {
      NrOfCols = _worldMapService.GetNrOfColsAndRows().Item1,
      NrOfRows = _worldMapService.GetNrOfColsAndRows().Item2
    };

    return Ok(settings);
  }

}
