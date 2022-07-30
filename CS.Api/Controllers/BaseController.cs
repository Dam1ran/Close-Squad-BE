using CS.Application.Utils;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;
[ApiController]
[Route("[controller]")]
[ProducesErrorResponseType(typeof(void))]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class BaseController : ControllerBase {
  protected readonly IMediator _mediator;

  public BaseController(IMediator mediator) {
    _mediator = Check.NotNull(mediator, nameof(mediator));
  }
}