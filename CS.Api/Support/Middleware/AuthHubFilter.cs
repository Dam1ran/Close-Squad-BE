using CS.Core.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Support.Middleware;
public class AuthHubFilter : IHubFilter {
  public async ValueTask<object?> InvokeMethodAsync(
    HubInvocationContext invocationContext,
    Func<HubInvocationContext, ValueTask<object?>> next) {

    var expiry = invocationContext.Context.User?.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
    if (!string.IsNullOrWhiteSpace(expiry) && long.TryParse(expiry, out long seconds)) {

      var expiryDate = DateTimeOffset.FromUnixTimeSeconds(seconds);
      if (expiryDate.IsInFuture()) {
        return await next(invocationContext);
      }
    }

    await invocationContext.Hub.Clients.Caller.SendAsync("OnSessionExpired");
    return ValueTask.FromResult<object?>(null);
  }

  public Task OnConnectedAsync(
    HubLifetimeContext context,
    Func<HubLifetimeContext, Task> next)
      => next(context);

  public Task OnDisconnectedAsync(
    HubLifetimeContext context,
    Exception? exception,
    Func<HubLifetimeContext, Exception?, Task> next)
      => next(context, exception);

}
