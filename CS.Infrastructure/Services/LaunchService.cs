using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.Hosting;

namespace CS.Infrastructure.Services;
public class LaunchService : IHostedService {

  private readonly IWorldMapService _worldMapService;
  private readonly ICharacterService _characterService;
  private readonly ISkillService _skillService;

  public LaunchService(
    IWorldMapService worldMapService,
    ICharacterService characterService,
    ISkillService skillService) {
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _skillService = Check.NotNull(skillService, nameof(skillService));
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    _worldMapService.Init();
    _skillService.Init();
    _characterService.Init();
    await Task.CompletedTask;
  }

  public Task StopAsync(CancellationToken cancellationToken) {
    throw new NotImplementedException(); // TODO ...
  }

}
