using CS.Core.Entities.Abstractions;
using CS.Core.Models;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IHubSender {

  public void EnqueueSystemMessage(Nickname playerRecipientNickname, string message);

}
