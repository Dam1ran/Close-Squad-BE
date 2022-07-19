using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CS.Api.Support.Validation;

public sealed class ModelStateCheckFilter : IActionFilter {
  public void OnActionExecuted(ActionExecutedContext context) { }

  public void OnActionExecuting(ActionExecutingContext context) {
    if (!context.ModelState.IsValid) {
      context.Result = new BadRequestObjectResult(context.ModelState);
    }
  }
}