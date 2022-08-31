using System.Net;
using CS.Api.Support.Exceptions;
using CS.Application.Support.Utils;

namespace CS.Api.Support.Middleware;
public class CatchMiddlewareExceptions {

  private readonly RequestDelegate _next;
  private readonly ILogger<CatchMiddlewareExceptions> _logger;

  public CatchMiddlewareExceptions(RequestDelegate next, ILogger<CatchMiddlewareExceptions> logger) {
    _next = Check.NotNull(next, nameof(next));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task InvokeAsync(HttpContext context) {
    try {
      await _next.Invoke(context);
    } catch (ArgumentNullException ex) {
      _logger.LogError(ex.Message);
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    } catch (MiddlewareException ex) {
      _logger.LogError(ex.Message);
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
    }
  }

}
