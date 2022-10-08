
using CS.Application.Enums;

namespace CS.Application.Models;
public class ChatMessage {
  public ChatMessageType Type {get; set;} = ChatMessageType.Nearby;
  public string Text { get; set; } = "";
  public ChatPlayerDto? ChatPlayerDto { get; set; }
}
