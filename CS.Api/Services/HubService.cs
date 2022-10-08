using CS.Api.Communications;
using CS.Application.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;


namespace CS.Api.Services;
public class HubService : IHubService {
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly IHubContext<MainHub, ITypedHubClient> _mainHubContext;
  private readonly IWorldMapService _worldMapService;
  private readonly ITickService _tickService;


  public HubService(
    IPlayerService playerService,
    ICharacterService characterService,
    IHubContext<MainHub, ITypedHubClient> mainHubContext,
    IWorldMapService worldMapService,
    ITickService tickService) {
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _mainHubContext = Check.NotNull(mainHubContext, nameof(mainHubContext));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _tickService = Check.NotNull(tickService, nameof(tickService));
  }

  public async Task SendAllUpdateQuadrantPlayerList(Player player, bool toSelf = false) {
    if (player.QuadrantIndex.HasValue) {
      var players = _playerService.GetPlayersInQuadrant(player.QuadrantIndex.Value).Where(p => p.Id != player.Id || toSelf);
      foreach (var _player in players) {
        await _mainHubContext.Clients.User(_player.Nickname)
          .SetNearbyGroup(players.Where(p => p.Id != _player.Id).Select(p => ChatPlayerDto.FromPlayer(p)));
      }
    }
  }

  public async Task SetCurrentPlayer(Player player) {
    if (player.QuadrantIndex.HasValue) {
      var indexes = _worldMapService.GetQuadrantsIndexesAround(player.QuadrantIndex.Value, 2)
        .Select(i => i.ToString()).ToList();
      player.QuadrantsUrl = indexes;
    }
    await _mainHubContext.Clients.User(player.Nickname).SetCurrentPlayer(PlayerDto.FromPlayer(player));
  }

}
