using CS.Api.Support.Attributes;
using CS.Api.Support.Models;
using CS.Application.Models;
using CS.Application.Queries.Announcement;
using CS.Core.Support;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
public class AnnouncementController : BaseController {
  public AnnouncementController(IMediator mediator) : base(mediator) {}

  [HttpGet]
  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._1_Minute_InSeconds, MaxRequests = 6, By = LimitRequestsType.IpAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(IEnumerable<ServerAnnouncementDto>), StatusCodes.Status200OK)]
  public Task<IEnumerable<ServerAnnouncementDto>> Get(CancellationToken cancellationToken) =>
    _mediator.Send(new GetServerAnnouncementsQuery(), cancellationToken);

  [HttpPost] // admin and GM only
  [LimitRequests(TimeWindowInSeconds = Core_TimeConstants._5_Minutes_InSeconds, MaxRequests = 1, By = LimitRequestsType.RoleAndEndpoint)]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(ServerAnnouncementDto), StatusCodes.Status200OK)]
  public Task<ServerAnnouncementDto> Create([FromBody] CreateServerAnnouncementViewModel model, CancellationToken cancellationToken) =>
    _mediator.Send(model.ToCommand(), cancellationToken);

}
