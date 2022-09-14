using CS.Api.Controllers.Abstractions;
using CS.Api.Support;
using CS.Api.Support.Attributes;
using CS.Api.Support.Models;
using CS.Application.Commands.Announcement;
using CS.Application.DataTransferObjects;
using CS.Application.Queries.Announcement;
using CS.Core.Support;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers.Administration;
public class AnnouncementController : BaseController {
  public AnnouncementController(IMediator mediator) : base(mediator) {}

  [HttpGet("announcements")]
  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.IpAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(IEnumerable<ServerAnnouncementDto>), StatusCodes.Status200OK)]
  public Task<IEnumerable<ServerAnnouncementDto>> Get(CancellationToken cancellationToken) =>
    _mediator.Send(new GetServerAnnouncementsQuery(), cancellationToken);

  [HttpPost("create")]
  [Authorize(Policy = Api_Constants.ManagementPolicy)]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._5_Minutes_InSeconds, MaxRequests = 1, By = LimitRequestsType.RoleAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> Create([FromBody] CreateServerAnnouncementViewModel model, CancellationToken cancellationToken) {
    await _mediator.Send(model.ToCommand(), cancellationToken);

    return NoContent();
  }

  [HttpDelete("delete/{id}")]
  [Authorize(Policy = Api_Constants.ManagementPolicy)]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 2, By = LimitRequestsType.RoleAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken) {
    if (id > 0) {
      await _mediator.Send(new DeleteAnnouncementCommand(id), cancellationToken);
    }

    return NoContent();
  }

}
