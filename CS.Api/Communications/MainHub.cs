using System.Diagnostics;
using System.Text.RegularExpressions;
using CS.Api.Communications.Models;
using CS.Api.Support;
using CS.Application.Enums;
using CS.Application.Models;
using CS.Application.Models.Dialog;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.Enums;
using CS.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
public partial class MainHub : Hub<ITypedHubClient> {

  private readonly ILogger<MainHub> _logger;
  private readonly ICacheService _cacheService;
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;
  private readonly ICharacterEngine _characterEngine;
  private readonly IWorldMapService _worldMapService;
  private readonly IHubService _hubService;

  public MainHub(
    ILogger<MainHub> logger,
    ICacheService cacheService,
    IPlayerService playerService,
    ICharacterService characterService,
    ICharacterEngine characterEngine,
    IWorldMapService worldMapService,
    IHubService hubService)
  {
    _logger = Check.NotNull(logger, nameof(logger));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
    _characterEngine = Check.NotNull(characterEngine, nameof(characterEngine));
    _worldMapService = Check.NotNull(worldMapService, nameof(worldMapService));
    _hubService = Check.NotNull(hubService, nameof(hubService));
  }

  public async Task SendChatMessage(ChatMessage chatMessage) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    if (await IsBanned()) {
      await SendSystemChatMessage("User banned.");
      return;
    }

    if (chatMessage.Text.Length > 255) {
      chatMessage.Text = chatMessage.Text.Substring(0, 255);
    }

    chatMessage.Text = Regex.Replace(chatMessage.Text, @"^(\/[n|w|p|c|s]) ", "").Trim();

    if (!Enum.IsDefined(typeof(ChatMessageType), chatMessage.Type) || string.IsNullOrWhiteSpace(chatMessage.Text)) {
      return;
    }

    var isShout = chatMessage.Type == ChatMessageType.Shout;
    var playerAllowedToShout = true; // TODO
    if (isShout && !playerAllowedToShout) {
      await SendSystemChatMessage("Shout not available at this time.");
      return;
    }

    var playerHasBanChat = false; // TODO
    if (playerHasBanChat) {
      await SendSystemChatMessage($"Chat is silenced for {12}m.");
      return;
    }

    var cachedMessage = await _cacheService
      .GetAsync<CachedChatMessage>(
        isShout ? CacheGroupKeyConstants.UserShoutThrottle : CacheGroupKeyConstants.UserChatThrottle,
        Context.UserIdentifier!);

    if (
      cachedMessage is not null &&
      (cachedMessage.Text.Equals(chatMessage.Text) ||
      cachedMessage.SentAt.AddSeconds(1) > DateTimeOffset.UtcNow)) {
      return;
    }

    if (isShout && cachedMessage is not null) {
      await SendSystemChatMessage($"Shout on cool down for {120 - (int)(DateTimeOffset.UtcNow - cachedMessage.SentAt).TotalSeconds}s.");
      return;
    }

    await _cacheService
      .SetAsync(
        isShout ? CacheGroupKeyConstants.UserShoutThrottle : CacheGroupKeyConstants.UserChatThrottle,
        Context.UserIdentifier!,
        new CachedChatMessage() { Text = chatMessage.Text, SentAt = DateTimeOffset.UtcNow, Type = chatMessage.Type },
        absoluteExpiration: DateTimeOffset.UtcNow.AddMinutes(isShout ? 2 : 1));

    switch (chatMessage.Type) {
      case ChatMessageType.Nearby: {
        await ProcessNearbyMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Whisper: {
        await ProcessWhisperMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Party: {
        await ProcessPartyMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Clan: {
        await ProcessClanMessage(currentPlayer, chatMessage);
        return;
      }
      case ChatMessageType.Shout: {
        await ProcessShoutMessage(currentPlayer, chatMessage);
        return;
      }
      default: return;
    }
  }

  // public async Task SendChatCommand(ChatCommand chatCommand) {
  //   Debug.WriteLine(chatCommand.Text);
  // }

  [Authorize(Policy = Api_Constants.AdministrationPolicy)]
  // [Authorize(Policy = Api_Constants.ManagementPolicy)]
  // [Authorize(Policy = Api_Constants.GameMasterPolicy)]
  // public async Task SendBanPlayer(Player player) {
  //   Debug.WriteLine(player.Nickname);
  // }

  public override async Task OnConnectedAsync() {

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      await Clients.Caller.SendServerDialog(new() {
        Message = "Create a character to use chat.",
        CanBeClosed = true,
        DialogType = DialogType.Warning,
        Title = "No character..."
      });
    } else {
      currentPlayer = _playerService.ClearLogoutTime(currentPlayer);
      await _hubService.SetCurrentPlayer(currentPlayer);
      await SendPlayerCharacters(currentPlayer);
    }

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception) {

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is not null) {
      if (currentPlayer.QuadrantIndex.HasValue) {
        await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);
        currentPlayer = _playerService.UpdatePlayerQuadrant(currentPlayer, null);
      }

      _playerService.SetLogoutTime(currentPlayer);
    }

    await SendSystemChatMessage("Disconnected.");
    await base.OnDisconnectedAsync(exception);
  }


  public async Task PlayerJumpTo(CharacterCall characterCall) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var character = _characterService.GetCharacterOf(currentPlayer, characterCall.CharacterId);
    if (character is null) {
      return;
    }

    if (currentPlayer.QuadrantIndex == character!.QuadrantIndex) {
      return;
    }

    await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);

    var jumpedPlayer = _playerService.UpdatePlayerQuadrant(currentPlayer, character.QuadrantIndex);
    await _hubService.SetCurrentPlayer(jumpedPlayer);
    await _hubService.SendAllUpdateQuadrantPlayerList(jumpedPlayer, true);
  }

  public async Task PlayerLeaveQuadrant() {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    await _hubService.SendAllUpdateQuadrantPlayerList(currentPlayer);

    await Clients.Caller.SetNearbyGroup(Enumerable.Empty<ChatPlayerDto>());
    var player = _playerService.UpdatePlayerQuadrant(currentPlayer, null);
    await _hubService.SetCurrentPlayer(player);
  }

  public async Task CharacterToggle(CharacterCall characterCall) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var character = _characterService.GetCharacterOf(currentPlayer, characterCall.CharacterId);
    if (character is null) {
      return;
    }

    var toggledCharacter = _characterService.Toggle(currentPlayer, character);
    if (toggledCharacter is not null) {
      if (currentPlayer?.QuadrantIndex == toggledCharacter.QuadrantIndex) {
        await PlayerLeaveQuadrant();
      }

      await Clients.Caller.UpdateCharacter(new { Id = toggledCharacter.Id, CharacterStatus = toggledCharacter.CharacterStatus });
    }

  }

  public async Task CharacterTravelTo(CharacterTravelCall characterTravelCall) {
    if (!Enum.IsDefined(typeof(TravelDirection), characterTravelCall.TravelDirection)) {
      return;
    }

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var character = _characterService.GetCharacterOf(currentPlayer, characterTravelCall.CharacterId);
    if (character is null || !character.CanTravel()) {
      return;
    }


    // TODO: check for quadrant requirements...


    var characterStatus = character.CharacterStatus;

    var travelingCharacter = await _characterService.Update(currentPlayer, character, (key, existing) =>
    {
      existing.CharacterStatus = CharacterStatus.Traveling;
      return existing;
    });
    if (travelingCharacter is null) {
      return;
    }

    await Clients.Caller.UpdateCharacter(new { Id = travelingCharacter.Id, CharacterStatus = travelingCharacter.CharacterStatus });

    if (characterStatus != travelingCharacter.CharacterStatus && travelingCharacter.CharacterStatus == CharacterStatus.Traveling) {
      var secondsToTravel = _characterEngine.TravelTo(characterTravelCall.TravelDirection, travelingCharacter, currentPlayer);
    }

  }

  public async Task ScoutQuadrant(CharacterScoutCall characterScoutCall) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var character = _characterService.GetCharacterOf(currentPlayer, characterScoutCall.CharacterId);
    if (character is null /* || no consumable */) {
      return;
    }

    await _characterService.Update(currentPlayer, character, (key, existing) => {
      // existing.Inventory.Decrement(consumable)
      return existing;
    });

    // await Clients.Caller.UpdateCharacterInventory(new { Id = character.Id, ...items });

    var quadrant = _worldMapService.GetQuadrantByIndexIfExists(characterScoutCall.QuadrantIndex);
    if (quadrant is null) {
      return;
    }

    var report = new ScoutQuadrantReport() {
      QuadrantIndex = quadrant.Index,
      Area = quadrant.Area,
      Name = quadrant.Name,
      Characters = _characterService
        .GetCharactersInQuadrant(quadrant.Index)
        .Where(c => c.CharacterClass != CharacterClass.Assassin)
        .Select(c => CharacterSimpleDto.FromCharacter(c))
      // others
    };

    await Clients.Caller.SendScoutQuadrantReport(report);

  }

}
