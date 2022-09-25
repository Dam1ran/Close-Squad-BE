using CS.Api.Communications.Models.Enums;

namespace CS.Api.Communications.Models;
public class CachedChatMessage {
  public string Text { get; set; } = string.Empty;
  public ChatMessageType Type {get; set;}
  public DateTimeOffset SentAt { get; set; }
}
