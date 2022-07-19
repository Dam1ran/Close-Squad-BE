using CS.Application.Exceptions;
using CS.Application.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CS.Api.Support.Filters;
public class ExceptionFilter : IAsyncExceptionFilter {
  private readonly ILogger<ExceptionFilter> _logger;

  public ExceptionFilter(ILogger<ExceptionFilter> logger) {
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public Task OnExceptionAsync(ExceptionContext context) {
    _logger.LogError(context.Exception, "An error occurred");
    context.Result = GetResult(context.Exception);
    return Task.CompletedTask;
  }

  public static IActionResult GetResult(Exception ex) {
    return ex switch {
      // AlreadyExistsException => new StatusCodeResult(StatusCodes.Status303SeeOther),
      // DomainValidationException => new BadRequestObjectResult(GetErrorBody(ex)),
      // UnauthorizedException => new ForbidResult(),
      // NotAcceptableException => new StatusCodeResult(StatusCodes.Status406NotAcceptable),
      // ConflictException => new ConflictObjectResult(GetErrorBody(ex)),
      NotFoundException => new NotFoundObjectResult(GetErrorBody(ex)),
      _ => ex.InnerException != null
          ? GetResult(ex.InnerException)
          : new StatusCodeResult(StatusCodes.Status500InternalServerError),
    };
  }

  public static object GetErrorBody(Exception ex) => new { errorMessage = ex.Message, data = ex.Data };

}