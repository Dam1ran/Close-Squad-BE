using CS.Application.Utils;
using Microsoft.AspNetCore.Antiforgery;

namespace CS.Api.Support;
public class ExceptionHandlerMiddleware {
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlerMiddleware> _logger;

  public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger) {
    _next = Check.NotNull(next, nameof(next));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task Invoke(HttpContext context) {
    try {
      await _next.Invoke(context);
    } catch (AntiforgeryValidationException ex) {
      _logger.LogWarning(ex.Message);
    }
  }
}
