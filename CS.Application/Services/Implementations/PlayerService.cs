using System.Collections.Concurrent;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Services.Interfaces;
using CS.Core.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace CS.Application.Services.Implementations;
public class PlayerService : IPlayerService {

  private readonly IServiceProvider _serviceProvider;
  private readonly ICacheService _cacheService;
  private readonly IWorldMapService _worldMapService;

  private readonly ConcurrentDictionary<string, Player> Players = new();

  public PlayerService(
    ICacheService cacheService,
    IServiceProvider serviceProvider,
    IWorldMapService worldMapService) {
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
  }

  public async Task<Player?> GetPlayer(Nickname nickname, bool store = true, CancellationToken cancellationToken = default) {

    if (Players.TryGetValue(nickname.ValueLowerCase, out Player? storedPlayer)) {
      return storedPlayer;
    }

    if (!store) {
      return null;
    }

    using (var scope = _serviceProvider.CreateScope()) {
      var _csUserRepo = scope.ServiceProvider.GetRequiredService<ICsUserRepository>();
      var csUser = await _csUserRepo.FindByNicknameWithPlayerQuadrantAsync(nickname, cancellationToken);
      if (csUser is null) {
        return null;
      }

      if (!csUser.Player.QuadrantId.HasValue) {
        var startingQuadrant = await _worldMapService.GetStartingQuadrantAsync(cancellationToken);
        csUser.Player.Quadrant = startingQuadrant;
        await _csUserRepo.SaveChangesAsync(cancellationToken);
      }

      return Players
        .AddOrUpdate(
          nickname.ValueLowerCase,
          csUser.Player,
          (key, existing) => csUser.Player);

    }

  }

  public void RemovePlayer(Nickname nickname) {
    Players.TryRemove(nickname.ValueLowerCase, out Player? outPlayer);
  }

  public async Task<IReadOnlyList<string>> GetPlayerNicknamesInBigQuadrantOf(Player player, CancellationToken cancellationToken = default) {
    var players = new List<string>();
    var _player = await GetPlayer(player.Nickname, true, cancellationToken);
    if (_player is null || _player.Quadrant is null) {
      return players;
    }

    var indexPairs = _worldMapService.GetQuadrantsIndexesAround(player.Quadrant!);
    var playersSnapshot = Players.Values;
    foreach (var pair in indexPairs) {
      var nicknames = playersSnapshot
        .Where(ps => ps.Quadrant!.XIndex == pair.Item1 && ps.Quadrant!.YIndex == pair.Item2)
        .Select(p => p.Nickname.Value);

      players.AddRange(nicknames);
    }

    return players;
  }

  public async Task<IEnumerable<Player>> GetPlayersInQuadrantOf(Player player, CancellationToken cancellationToken = default) {
    var players = new List<Player>();
    var _player = await GetPlayer(player.Nickname, true, cancellationToken);
    if (_player is null || _player.Quadrant is null) {
      return players;
    }

    players.AddRange(
      Players.Values
        .Where(ps =>
          ps.Quadrant!.XIndex == _player.Quadrant.XIndex &&
          ps.Quadrant!.YIndex == _player.Quadrant.YIndex));

    return players;

  }

}
