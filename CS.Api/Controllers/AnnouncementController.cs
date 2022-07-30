using CS.Api.Support.Models;
using CS.Application.Commands.Announcement;
using CS.Application.Models;
using CS.Application.Queries.Announcement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
public class AnnouncementController : BaseController {
  public AnnouncementController(IMediator mediator) : base(mediator) {}

  [HttpGet]
  [AllowAnonymous]
  [IgnoreAntiforgeryToken]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(IEnumerable<ServerAnnouncementDto>), StatusCodes.Status200OK)]
  public Task<IEnumerable<ServerAnnouncementDto>> Get(CancellationToken cancellationToken) =>
    _mediator.Send(new GetServerAnnouncementsQuery(), cancellationToken);

  [HttpPost]
  [ProducesDefaultResponseType]
  [ProducesResponseType(typeof(ServerAnnouncementDto), StatusCodes.Status200OK)]
  public Task<ServerAnnouncementDto> Create([FromBody] CreateServerAnnouncementViewModel model, CancellationToken cancellationToken) =>
    _mediator.Send(model.ToCommand(), cancellationToken);

}
