using System.Text.RegularExpressions;
using CS.Api.Communications.Models;
using CS.Api.Communications.Models.Enums;
using CS.Api.Support;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Constants;
using CS.Application.Support.Utils;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
public partial class MainHub : Hub<ITypedHubClient> {

  private readonly ILogger<MainHub> _logger;
  private readonly ICacheService _cacheService;
  private readonly IPlayerService _playerService;
  private readonly ICharacterService _characterService;

  public MainHub(
    ILogger<MainHub> logger,
    ICacheService cacheService,
    IPlayerService playerService,
    ICharacterService characterService) {
    _logger = Check.NotNull(logger, nameof(logger));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
    _characterService = Check.NotNull(characterService, nameof(characterService));
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
      await SendSystemChatMessage("Create a character to use chat.");
    } else {
      await SendPlayerCharacters(currentPlayer);
    }

    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer?.Quadrant is not null) {
      await SendOthersLeavingQuadrant(currentPlayer);
    }
    _playerService.ClearPlayer(new Nickname(Context.UserIdentifier!)); // add delay for reconnect scenario to cancel removing
    // clear characters // add delay for reconnect scenario to cancel removing
    await SendSystemChatMessage("Disconnected.");
    await base.OnDisconnectedAsync(exception);
  }


  public async Task PlayerJumpTo(string characterNicknameValue) {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null || Nickname.IsWrongNickname(characterNicknameValue, out Nickname? characterNickname)) {
      return;
    }

    var characterQuadrant = _characterService.GetCharacterQuadrant(currentPlayer.Nickname, characterNickname!);
    if (characterQuadrant is null || currentPlayer.Quadrant?.Id == characterQuadrant.Id) {
      return;
    }

    if (currentPlayer.Quadrant is not null) {
      await SendOthersLeavingQuadrant(currentPlayer);
    }

    var jumpedPlayer = (await _playerService.UpdatePlayerQuadrant(currentPlayer.Nickname, characterQuadrant))!;
    await Clients.Caller.SetCurrentPlayer(PlayerDto.FromPlayer(jumpedPlayer));
    await SendEnteringQuadrant(jumpedPlayer);

  }

  public async Task PlayerLeaveQuadrant() {
    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    if (currentPlayer.Quadrant is not null) {
      await SendOthersLeavingQuadrant(currentPlayer);
    }

    await Clients.Caller.SetNearbyGroup(Enumerable.Empty<ChatPlayerDto>());
    var player = (await _playerService.UpdatePlayerQuadrant(currentPlayer.Nickname, null))!;
    await Clients.Caller.SetCurrentPlayer(PlayerDto.FromPlayer(player));

  }

  public async Task CharacterToggle(string characterNicknameValue) {
    if (Nickname.IsWrongNickname(characterNicknameValue, out Nickname? characterNickname)) {
      return;
    }

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var character = _characterService.Toggle(currentPlayer.Nickname, characterNickname!);
    if (character is not null) {
      if (currentPlayer.Quadrant?.Id == character.Quadrant.Id) {
        await PlayerLeaveQuadrant();
      }

      await Clients.Caller.UpdateCharacter(new { Id = character.Id, CharacterStatus = character.CharacterStatus });
    }


  }

  public async Task CharacterTravelTo(CharacterTravelCall characterTravelCall) {
    if (Nickname.IsWrongNickname(characterTravelCall.CharacterNickname, out Nickname? characterNickname)) {
      return;
    }

    var currentPlayer = await GetCurrentPlayer();
    if (currentPlayer is null) {
      return;
    }

    var travelingCharacter = await _characterService.SetTraveling(currentPlayer, characterNickname!);
    if (travelingCharacter is null) {
      return;
    }

    // await _characterManager.

    await Clients.Caller.UpdateCharacter(new { Id = travelingCharacter.Id, CharacterStatus = travelingCharacter.CharacterStatus });

  }

}
