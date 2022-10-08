using System.Collections.Concurrent;
using System.Diagnostics;
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
  private readonly ITickService _tickService;

  private readonly ConcurrentDictionary<string, Player> Players = new();
  private const int PlayerBufferClearIntervalSeconds = 60;

  public PlayerService(
    ICacheService cacheService,
    IServiceProvider serviceProvider,
    IWorldMapService worldMapService,
    ITickService tickService) {
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _serviceProvider = Check.NotNull(serviceProvider, nameof(serviceProvider));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _tickService = Check.NotNull(tickService, nameof(tickService));
    _tickService.on_60s_tick += ClearLoggedOutPlayers;
  }

  public async Task<Player> GetOrCreatePlayerAsync(Nickname playerNickname, CancellationToken cancellationToken = default) {
    using var scope = _serviceProvider.CreateScope();
    var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
    var player = await _playerRepo.FindByNicknameWithCharactersAsync(playerNickname, cancellationToken);
    if (player is not null) {
      return player;
    }

    player = new Player(playerNickname);
    player.LogoutAt = DateTimeOffset.UtcNow;
    await _playerRepo.AddAsync(player, cancellationToken);
    await _playerRepo.SaveChangesAsync(cancellationToken);

    return player;

  }

  public async Task<Player?> GetPlayerAsync(Nickname nickname, bool store = true) {

    if (Players.TryGetValue(nickname.ValueLowerCase, out Player? storedPlayer)) {
      return storedPlayer;
    }

    if (!store) {
      return null;
    }

    using (var scope = _serviceProvider.CreateScope()) {
      var _playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
      var player = await _playerRepo.FindByNicknameAsNoTrackingAsync(nickname);
      if (player is null) {
        return null;
      }

      return Players
        .AddOrUpdate(
          nickname.ValueLowerCase,
          player,
          (key, existing) => player);

    }

  }

  public void SetLogoutTime(Player player) {
    Players.AddOrUpdate(
      player.Nickname.ValueLowerCase,
      player,
      (key, existing) =>
        {
          existing.LogoutAt = DateTimeOffset.UtcNow;
          return existing;
        });
  }

  public IReadOnlyList<string> GetPlayerNicknamesInBigQuadrantOf(Player player) {
    var players = new List<string>();
    if (!player.QuadrantIndex.HasValue) {
      return players;
    }

    var indexes = _worldMapService.GetQuadrantsIndexesAround(player.QuadrantIndex.Value);
    var playersSnapshot = Players.Values;
    foreach (var index in indexes) {
      var nicknames = playersSnapshot
        .Where(ps => ps.QuadrantIndex == index)
        .Select(p => p.Nickname.Value);

      players.AddRange(nicknames);
    }

    return players;
  }

  public List<Player> GetPlayersInQuadrant(uint quadrantIndex) =>
    Players.Values.Where(ps => ps.QuadrantIndex == quadrantIndex).ToList();

  public Player UpdatePlayerQuadrant(Player player, uint? quadrantIndex) {

    return Players.AddOrUpdate(
      player.Nickname.ValueLowerCase,
      player,
      (key, existing) =>
        {
          existing.QuadrantIndex = quadrantIndex;
          return existing;
        });

  }

  public Player ClearLogoutTime(Player player) {

    return Players.AddOrUpdate(
      player.Nickname.ValueLowerCase,
      player,
      (key, existing) =>
        {
          existing.LogoutAt = null;
          return existing;
        });
  }

  private void ClearLoggedOutPlayers(object? sender, EventArgs e) {
    foreach (var player in Players) {
      if (player.Value.LogoutAt.HasValue && player.Value.LogoutAt.Value.AddSeconds(PlayerBufferClearIntervalSeconds) < DateTimeOffset.UtcNow) {
        Players.TryRemove(player.Key, out Player? outPlayer);
        Debug.WriteLine($"Cleared key: \"{player.Key}\" from Players cache.");
      }

    }
  }


}
