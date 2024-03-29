using System.Text.RegularExpressions;
using CS.Application.Enums;
using CS.Application.Models;
using CS.Application.Services.Abstractions;
using CS.Core.Entities;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Communications;
public partial class MainHub : Hub<ITypedHubClient> {

  private async Task SendPlayerCharacters(Player player) {
    var characters = await _characterService.GetCharactersOf(player);
    await Clients.Caller.SetCharacters(characters.Select(CharacterDto.FromCharacter));
  }

  private async Task SendSystemChatMessage(string text) {
    await Clients.Caller.ReceiveChatMessage(
      new ChatMessage {
        Type = ChatMessageType.General,
        Text = text,
        ChatPlayerDto = new () { Id = -1, Nickname = "*System*", ClanName = "System", ClanIcon = "✳" }
      });

  }

  private async Task ProcessNearbyMessage(Player player, ChatMessage chatMessage) {

    var players = _playerService.GetPlayerNicknamesInBigQuadrantOf(player);
    await Clients.Users(players).ReceiveChatMessage(new ChatMessage {
      Type = ChatMessageType.Nearby,
      Text = chatMessage.Text,
      ChatPlayerDto = ChatPlayerDto.FromPlayer(player)
    });

  }

  private async Task ProcessWhisperMessage(Player player, ChatMessage chatMessage) {
    var nicknameValue = chatMessage.Text.Split(" ")[0];
    if (string.IsNullOrWhiteSpace(nicknameValue)) {
      return;
    }

    if (Nickname.IsWrongNickname(nicknameValue, out Nickname? nickname)) {
      await SendSystemChatMessage("Wrong nickname.");
      return;
    }

    var playerTo = await _playerService.GetPlayerAsync(nickname!, false);
    if (playerTo is null) {
      await SendSystemChatMessage($"{nickname!.Value} offline.");
      return;
    }

    if (nickname!.ValueLowerCase == Context.UserIdentifier!.ToLowerInvariant()) {
      return;
    }

    var text = Regex.Replace(chatMessage.Text, $"^{nickname.Value} ", "");
    await Clients.User(playerTo.Nickname.Value).ReceiveChatMessage(new ChatMessage {
      Type = ChatMessageType.Whisper,
      Text = $" -> {text}",
      ChatPlayerDto = ChatPlayerDto.FromPlayer(player)
    });

    await Clients.Caller.ReceiveChatMessage(new ChatMessage {
      Type = ChatMessageType.Whisper,
      Text = $" -> {nickname.Value}: {text}",
      ChatPlayerDto = ChatPlayerDto.FromPlayer(player)
    });

  }

  private async Task ProcessPartyMessage(Player player, ChatMessage chatMessage) {
    
  }
  private async Task ProcessClanMessage(Player player, ChatMessage chatMessage) {
    
  }
  private async Task ProcessShoutMessage(Player player, ChatMessage chatMessage) {
    await Clients.All.ReceiveChatMessage(new ChatMessage {
      Type = ChatMessageType.Shout,
      Text = chatMessage.Text,
      ChatPlayerDto = ChatPlayerDto.FromPlayer(player)
    });
  }

  private async Task<Player?> GetCurrentPlayer() => await _playerService.GetPlayerAsync(new Nickname(Context.UserIdentifier!));

  private async Task<Boolean> IsBanned() {
    var currentUser = new { IsBanned = false }; // TODO
    return currentUser.IsBanned;
  }
}
