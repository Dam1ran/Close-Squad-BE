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
internal partial class MainHub : Hub {

  private readonly ILogger<MainHub> _logger;
  private readonly ICacheService _cacheService;
  private readonly IPlayerService _playerService;

  public MainHub(
    ILogger<MainHub> logger,
    ICacheService cacheService,
    IPlayerService playerService) {
    _logger = Check.NotNull(logger, nameof(logger));
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
    _playerService = Check.NotNull(playerService, nameof(playerService));
  }

  public async Task SendChatMessage(ChatMessage chatMessage) {

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
        await ProcessNearbyMessage(chatMessage);
        return;
      }
      case ChatMessageType.Whisper: {
        await ProcessWhisperMessage(chatMessage);
        return;
      }
      case ChatMessageType.Party: {
        await ProcessPartyMessage(chatMessage);
        return;
      }
      case ChatMessageType.Clan: {
        await ProcessClanMessage(chatMessage);
        return;
      }
      case ChatMessageType.Shout: {
        await ProcessShoutMessage(chatMessage);
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
    await SendCurrentPlayer();
    await SendNearbyGroup();
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception) {
    await SendNearbyGroup(false);
    _playerService.RemovePlayer(new Nickname(Context.UserIdentifier!));
    await SendSystemChatMessage("Disconnected.");
    await base.OnDisconnectedAsync(exception);
  }

}
