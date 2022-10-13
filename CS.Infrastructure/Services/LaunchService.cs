using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CS.Infrastructure.Services;
public class LaunchService : IHostedService {

  private readonly IWorldMapService _worldMapService;
  private readonly ICharacterService _characterService;

  public LaunchService(
    IWorldMapService worldMapService,
    ICharacterService characterService) {
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    _worldMapService.Init();
    _characterService.Init();
    await Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken) {
    throw new NotImplementedException(); // TODO ...
  }

}
