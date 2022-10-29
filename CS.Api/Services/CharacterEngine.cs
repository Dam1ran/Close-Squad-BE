using System.Collections.Concurrent;
using CS.Api.Communications;
using CS.Application.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.Entities;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using CS.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Services;
public class CharacterEngine : ICharacterEngine {

  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly IHubContext<MainHub, ITypedHubClient> _mainHubContext;
  private readonly IWorldMapService _worldMapService;
  private readonly ITickService _tickService;
  private readonly IHubService _hubService;

  private readonly ConcurrentDictionary<string, DelayedTask> Tasks = new();

  private const double _1000MsSpeedMultiplier = 10.0F;

  public CharacterEngine(
    IPlayerService playerService,
    ICharacterService characterService,
    IHubContext<MainHub, ITypedHubClient> mainHubContext,
    IWorldMapService worldMapService,
    ITickService tickService,
    IHubService hubService) {
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _mainHubContext = Check.NotNull(mainHubContext, nameof(mainHubContext));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _tickService = Check.NotNull(tickService, nameof(tickService));
    _hubService = Check.NotNull(hubService, nameof(hubService));

    _tickService.on_1000ms_tick += StartDelayedTasks;
    _tickService.on_100ms_tick += CharactersTick;
  }

  public int TravelTo(TravelDirection travelDirection, Character character, Player player) {

    var hypotenuseHalfLength = 0.5;
    if (travelDirection == TravelDirection.NE
      || travelDirection == TravelDirection.SE
      || travelDirection == TravelDirection.SW
      || travelDirection == TravelDirection.NW) {
      hypotenuseHalfLength = 0.7071;
    }

    var arrivingQuadrantIndex  = _worldMapService.GetArrivingQuadrantIndex(character.QuadrantIndex, travelDirection);
    var currentQuadrantSpeedModifier = _worldMapService.GetQuadrantByIndex(character.QuadrantIndex).SpeedModifier;
    var arrivingQuadrantSpeedModifier = _worldMapService.GetQuadrantByIndex(arrivingQuadrantIndex).SpeedModifier;

    var travelLength = hypotenuseHalfLength / currentQuadrantSpeedModifier + hypotenuseHalfLength / arrivingQuadrantSpeedModifier;
    var secondsToTravel = (int)((travelLength * WorldMapService.QuadrantSizeInMeters) / (character.Stats.Speed.Current * _1000MsSpeedMultiplier));

    var previousQuadrantIndex = character.QuadrantIndex;

    var taskGuid = Guid.NewGuid().ToString();
    var task = new Task(async () =>
    {
      if (character.CanArrive()) {

        character.QuadrantIndex = arrivingQuadrantIndex;
        character.Status = CsEntityStatus.Awake;
        character.Position.SetLocationAndDestination(travelDirection);

        await _characterService.PersistAsync(character);

        await _mainHubContext.Clients
          .User(player.Nickname)
          .UpdateCharacter(new
          {
            Id = character.Id,
            CharacterStatus = character.Stats,
            QuadrantIndex = character.QuadrantIndex,
            X = character.Position.LocationX,
            Y = character.Position.LocationY,
            xDestination = character.Position.LocationX,
            yDestination = character.Position.LocationY,
          });

        await UpdatePlayerQuadrant(player, previousQuadrantIndex, character);

        Tasks.TryRemove(taskGuid, out _);

      }

    });

    Tasks.TryAdd(taskGuid, new DelayedTask(task, DateTimeOffset.UtcNow.AddSeconds(2))); // use secondsToTravel when done
    return secondsToTravel;

  }

  private async Task UpdatePlayerQuadrant(Player player, uint previousQuadrantIndex, Character updatedCharacter) {
    var currentPlayer = await _playerService.GetPlayerAsync(player.Nickname, false);
    if (currentPlayer is not null && currentPlayer.QuadrantIndex.HasValue && currentPlayer.QuadrantIndex.Value == previousQuadrantIndex) {
      _ =_hubService.SendAllUpdateQuadrantPlayerList(currentPlayer, false).ConfigureAwait(false);
      var updatedPlayer = _playerService.UpdatePlayerQuadrant(player, updatedCharacter.QuadrantIndex);
      if (updatedPlayer.QuadrantIndex.HasValue) {
        _ = _hubService.SendAllUpdateQuadrantPlayerList(updatedPlayer, true).ConfigureAwait(false);
        _ = _hubService.SetCurrentPlayer(updatedPlayer);
      }

    }

  }

  private void StartDelayedTasks(object? sender, EventArgs e) {
    foreach (var task in Tasks) {
      if (task.Value.ExecuteAt <= DateTimeOffset.UtcNow && !task.Value.Executing) {
        Tasks.AddOrUpdate(task.Key, task.Value, (key, existing) =>
        {
          existing.Executing = true;
          return existing;
        });

        Task.Run(() => task.Value.Task.Start()).ConfigureAwait(false);

      }
    }

  }

  private void CharactersTick(object? sender, EventArgs e) {

    foreach (var character in _characterService.GetAll()) {
      character.Tick();
    }

  }

}
