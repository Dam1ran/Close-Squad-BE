using System.Text.RegularExpressions;
using CS.Api.Communications.Models;
using CS.Api.Communications.Models.Enums;
using CS.Core.Entities;
using CS.Core.Exceptions;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
internal partial class MainHub : Hub {
  private async Task SendCurrentPlayer() {
    var storedPlayer = await _playerService.GetPlayer(new Nickname(Context.UserIdentifier!));
    await Clients.Caller.SendAsync("SetCurrentPlayer", PlayerDto.FromPlayer(storedPlayer!, Context.UserIdentifier!));
  }

  private async Task SendSystemChatMessage (string text) {
    await Clients.Caller.SendAsync("ReceiveChatMessage",
      new ChatMessage {
        Type = ChatMessageType.General,
        Text = text,
        ChatPlayer = new () { Id = -1, Nickname = "*System*", ClanName = "System", ClanIcon = "✳" }
      });

  }

  private async Task ProcessNearbyMessage(ChatMessage chatMessage) {
    var currentPlayer = await GetCurrentPlayer();
    var players = await _playerService.GetPlayerNicknamesInBigQuadrantOf(currentPlayer!);

    await Clients.Users(players).SendAsync("ReceiveChatMessage", new ChatMessage {
      Type = ChatMessageType.Nearby,
      Text = chatMessage.Text,
      ChatPlayer = ChatPlayerDto.FromPlayer((currentPlayer!))
    });

  }

  private async Task ProcessWhisperMessage(ChatMessage chatMessage) {
    var nicknameValue = chatMessage.Text.Split(" ")[0];
    if (string.IsNullOrWhiteSpace(nicknameValue)) {
      return;
    }

    Nickname nickname;
    try {
      nickname = new Nickname(nicknameValue);
    } catch (DomainValidationException) {
      await SendSystemChatMessage("Wrong nickname.");
      return;
    }

    var player = await _playerService.GetPlayer(nickname, false);
    if (player is null) {
      await SendSystemChatMessage($"{nickname.Value} offline.");
      return;
    }

    if (nickname.ValueLowerCase == Context.UserIdentifier!.ToLowerInvariant()) {
      return;
    }

    var text = Regex.Replace(chatMessage.Text, $"^{nickname.Value} ", "");
    await Clients.User(player.Nickname.Value).SendAsync("ReceiveChatMessage", new ChatMessage {
      Type = ChatMessageType.Whisper,
      Text = $" -> {text}",
      ChatPlayer = ChatPlayerDto.FromPlayer((await GetCurrentPlayer())!)
    });

    await Clients.Caller.SendAsync("ReceiveChatMessage", new ChatMessage {
      Type = ChatMessageType.Whisper,
      Text = $" -> {nickname.Value}: {text}",
      ChatPlayer = ChatPlayerDto.FromPlayer((await GetCurrentPlayer())!)
    });

  }

  private async Task ProcessPartyMessage(ChatMessage chatMessage) {
    
  }
  private async Task ProcessClanMessage(ChatMessage chatMessage) {
    
  }
  private async Task ProcessShoutMessage(ChatMessage chatMessage) {
    await Clients.All.SendAsync("ReceiveChatMessage", new ChatMessage {
      Type = ChatMessageType.Shout,
      Text = chatMessage.Text,
      ChatPlayer = ChatPlayerDto.FromPlayer((await GetCurrentPlayer())!)
    });
  }

  private async Task SendNearbyGroup(bool toSelf = true) {
    var currentPlayer = await GetCurrentPlayer();
    var players = await _playerService.GetPlayersInQuadrantOf(currentPlayer!);
    if (toSelf) {
      await Clients.Caller.SendAsync("NearbyGroup", players.Where(p => p.Id != currentPlayer!.Id).Select(p => ChatPlayerDto.FromPlayer(p)));
    }

    var playersExceptCaller = players.Where(p => p.Id != currentPlayer!.Id || toSelf);
    foreach (var player in playersExceptCaller) {
      await Clients.User(player.Nickname.Value)
        .SendAsync(
          "NearbyGroup",
          playersExceptCaller.Where(p => p.Id != player.Id).Select(p => ChatPlayerDto.FromPlayer(p))
        );
    }
  }

  private async Task<Player?> GetCurrentPlayer() => await _playerService.GetPlayer(new Nickname(Context.UserIdentifier!));

  private async Task<Boolean> IsBanned() {
    var currentUser = new { IsBanned = false }; // TODO
    return currentUser.IsBanned;
  }
}
