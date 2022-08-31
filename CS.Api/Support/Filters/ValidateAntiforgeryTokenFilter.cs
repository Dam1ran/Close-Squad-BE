using CS.Application.Support.Utils;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CS.Api.Support.Filters;
public class ValidateAntiforgeryTokenFilter : IAsyncAuthorizationFilter, IAntiforgeryPolicy {
  private readonly IAntiforgery _antiforgery;
  private readonly ILogger<ValidateAntiforgeryTokenFilter> _logger;

  public ValidateAntiforgeryTokenFilter(IAntiforgery antiforgery, ILogger<ValidateAntiforgeryTokenFilter> logger) {
    _antiforgery = Check.NotNull(antiforgery, nameof(antiforgery));
    _logger = Check.NotNull(logger, nameof(logger));
  }

  public async Task OnAuthorizationAsync(AuthorizationFilterContext context) {
    if (!context.IsEffectivePolicy<IAntiforgeryPolicy>(this)) {
      return;
    }

    try {
      await _antiforgery.ValidateRequestAsync(context.HttpContext);
    } catch (AntiforgeryValidationException e) {
      context.Result = new AntiforgeryValidationFailedResult();
      context.HttpContext.Response.Headers[Api_Constants.AntiforgeryCookiePlaceholder] = string.Empty;

      _logger.LogWarning(e.Message);
      _logger.LogWarning(e.InnerException?.Message);
    }
  }

}
