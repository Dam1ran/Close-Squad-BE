using CS.Application.Support.Utils;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CS.Infrastructure.Services;
public class LaunchService : IHostedService {

  private readonly IWorldMapService _worldMapService;

  public LaunchService(
    IWorldMapService worldMapService) {
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    _worldMapService.Init();
    await Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken) {
    throw new NotImplementedException(); // TODO ...
  }

}
