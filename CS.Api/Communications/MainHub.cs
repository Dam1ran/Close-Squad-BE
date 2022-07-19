using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
internal class MainHub : Hub {
  private readonly ILogger<MainHub> _logger;
  public MainHub(ILogger<MainHub> logger)
  {
    _logger = logger;
  }

  public async Task SendMessage(MainPayload payload) {
    _logger.LogInformation(payload?.Message, payload?.User);
    await Clients.All.SendAsync("ReceiveMessage", new MainPayload { User = "PLBLT", Message = "AHUETI" });
  }

  public override Task OnConnectedAsync() {
    // _logger.LogInformation(Context?.User?.Identity?.Name, " PIDAR CONNECTED");
    _logger.LogInformation($"{Context?.ConnectionId} PIDAR CONNECTED");
    return base.OnConnectedAsync();
  }

  public override Task OnDisconnectedAsync(Exception? exception) {
    _logger.LogInformation($"{Context?.ConnectionId} PIDAR DISCONNECTED");
    return base.OnDisconnectedAsync(exception);
  }
}

public class MainPayload {
  public string User { get; set; } = "";
  public string Message { get; set; } = "";
}