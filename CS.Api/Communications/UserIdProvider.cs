using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
public class UserIdProvider : IUserIdProvider {
  public string? GetUserId(HubConnectionContext connection) =>
    connection.User?.Claims.FirstOrDefault(x => x.Type == "nickname")?.Value;
}
