using CS.Api.Communications;
using CS.Application.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Enums;
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

    _tickService.on_100ms_tick += AggregateAndSendData;
  }

  public async Task SendAllUpdateQuadrantPlayerList(Player player, bool toSelf = false) {
    if (player.QuadrantIndex.HasValue) {
      var players = _playerService.GetPlayersInQuadrant(player.QuadrantIndex.Value).Where(p => p.Id != player.Id || toSelf);
      foreach (var _player in players) {
        await _mainHubContext.Clients.User(_player.Nickname)
          .SetNearbyGroup(players.Where(p => p.Id != _player.Id).Select(ChatPlayerDto.FromPlayer));
      }
    }
  }

  public async Task SendUpdateCharacters(long playerId, IEnumerable<CharacterDto> characters) {
    var playerNickname = _playerService.GetPlayerNickname(playerId);
    await _mainHubContext.Clients.User(playerNickname)
      .UpdateCharacters(characters);
  }

  public async Task SetCurrentPlayer(Player player) {
    if (player.QuadrantIndex.HasValue) {
      var indexes = _worldMapService.GetQuadrantsIndexesAround(player.QuadrantIndex.Value, 2)
        .Select(i => i.ToString()).ToList();
      player.QuadrantsUrl = indexes;
    }
    await _mainHubContext.Clients.User(player.Nickname).SetCurrentPlayer(PlayerDto.FromPlayer(player));
  }

  private void AggregateAndSendData(object? sender, EventArgs e) {

    var players = _playerService.GetPlayers();
    foreach (var group in _characterService.GetAll().GroupBy(c => c.PlayerId)) {
      var player = players.SingleOrDefault(p => p.Id == group.Key);
      if (player is null) {
        continue;
      }

      var data = new AggregatedDataDto {
        ClientCharacters = group.Select(CharacterDto.FromCharacter)
      };

      if (player.QuadrantIndex.HasValue) {
        data.CharactersInActiveQuadrant = _characterService
          .GetAll()
          .Where(
            c => c.QuadrantIndex == player.QuadrantIndex &&
            c.PlayerId != player.Id &&
            c.CharacterStatus != CharacterStatus.Astray)
          .Select(CharacterSimpleDto.FromCharacter);
      }

      // plus other stuff

      _mainHubContext.Clients.User(player.Nickname).SendAggregatedData(data);

    }

  }

}
