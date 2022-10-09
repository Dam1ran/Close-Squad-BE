using CS.Application.Models;

namespace CS.Application.Services.Abstractions;
public interface ITypedHubClient {

  Task SetCurrentPlayer(PlayerDto playerDto);
  Task SetCharacters(IEnumerable<CharacterDto> charactersDtos);
  Task ReceiveChatMessage(ChatMessage chatMessage);
  Task SetNearbyGroup(IEnumerable<ChatPlayerDto> chatPlayerDtos);
  Task UpdateCharacter(object characterDto);
  Task SendScoutQuadrantReport(ScoutQuadrantReport scoutQuadrantReport);
  Task Reconnect();

}
