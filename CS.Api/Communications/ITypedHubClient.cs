using CS.Api.Communications.Models;

namespace CS.Api.Communications;
public interface ITypedHubClient {

  Task SetCurrentPlayer(PlayerDto playerDto);
  Task SetCharacters(IEnumerable<CharacterDto> charactersDtos);
  Task ReceiveChatMessage(ChatMessage chatMessage);
  Task SetNearbyGroup(IEnumerable<ChatPlayerDto> chatPlayerDtos);
  Task UpdateCharacter(object characterDto);
  Task Reconnect();

}
