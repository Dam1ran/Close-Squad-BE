using CS.Api.Communications.Models.Enums;

namespace CS.Api.Communications.Models;
public class ChatMessage {
  public ChatMessageType Type {get; set;} = ChatMessageType.Nearby;
  public string Text { get; set; } = "";
  public ChatPlayerDto? ChatPlayerDto { get; set; }
}
