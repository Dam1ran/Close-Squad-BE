using CS.Core.Entities.Abstractions;
using CS.Core.Exceptions;

namespace CS.Core.Entities;
public class ServerAnnouncement : Entity {
  public DateTimeOffset CreatedAt { get; private set; }
  public string Message { get; private set; } = string.Empty;
  public ServerAnnouncement(string message) {
    if (string.IsNullOrEmpty(message.Trim())) {
      throw new DomainValidationException($"Inappropriate message for {nameof(ServerAnnouncement)}");
    }
    Message = message;
    CreatedAt = DateTimeOffset.Now;
  }

  protected ServerAnnouncement() {}

}