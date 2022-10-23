using System.Collections.Concurrent;
using CS.Api.Communications;
using CS.Application.Enums;
using CS.Application.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using CS.Core.ValueObjects;
using Microsoft.AspNetCore.SignalR;

namespace CS.Api.Services;
public class HubSender : IHubSender {

  private readonly IHubContext<MainHub, ITypedHubClient> _mainHubContext;
  private readonly PeriodicTimer _10ms_timer = new(TimeSpan.FromMilliseconds(10));
  private readonly ConcurrentDictionary<string, HubMessageDto> _messages = new();

  public HubSender(IHubContext<MainHub, ITypedHubClient> mainHubContext) {
    _mainHubContext = Check.NotNull(mainHubContext, nameof(mainHubContext));

    new Thread(Worker).Start();
  }

  private async void Worker() {
    while (await _10ms_timer.WaitForNextTickAsync()) {
      if (_messages.Count == 0) {
        continue;
      }
      foreach(var message in _messages) {
        await _mainHubContext.Clients.User(message.Value.Nickname).ReceiveChatMessage(
        new ChatMessage {
          Type = ChatMessageType.General,
          Text = message.Value.Message,
          ChatPlayerDto = new () { Id = -1, Nickname = "*System*", ClanName = "System", ClanIcon = "✳" }
        });

        _messages.TryRemove(message.Key, out var _);
      }
    };
  }

  public void EnqueueSystemMessage(Nickname playerRecipientNickname, string message) =>
    _messages.TryAdd(Guid.NewGuid().ToString(), new(playerRecipientNickname, message));
}

public class HubMessageDto {
  public HubMessageDto(string nickname, string message) {
    Nickname = nickname;
    Message = message;
  }
  public string Nickname { get; set; }
  public string Message { get; set; }
}
