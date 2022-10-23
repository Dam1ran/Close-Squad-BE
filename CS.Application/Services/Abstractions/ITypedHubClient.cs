using CS.Application.Models;
using CS.Core.Entities;

namespace CS.Application.Services.Abstractions;
public interface ITypedHubClient {

  Task SetCurrentPlayer(PlayerDto playerDto);
  Task SetCharacters(IEnumerable<CharacterDto> charactersDtos);
  Task ReceiveChatMessage(ChatMessage chatMessage);
  Task SetNearbyGroup(IEnumerable<ChatPlayerDto> chatPlayerDtos);
  Task UpdateCharacter(object characterPartialDto);
  Task UpdateCharacters(IEnumerable<object> characterPartialDtos);
  Task SendAggregatedData(AggregatedDataDto aggregatedDataDto);
  Task SendScoutQuadrantReport(ScoutQuadrantReport scoutQuadrantReport);
  Task SendServerDialog(ServerDialog dialog);
  Task SetBarShortcuts(IEnumerable<BarShortcut> barShortcuts);
  Task UpdateBarShortcut(BarShortcut barShortcut);
  Task UpdateBarShortcuts(IEnumerable<BarShortcut> barShortcuts);
  Task Reconnect();
  Task Disconnect();

}
