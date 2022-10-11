using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IPlayerService {

  Nickname GetPlayerNickname(long playerId);
  ICollection<Player> GetPlayers();
  Task<Player> GetOrCreatePlayerAsync(Nickname playerNickname, CancellationToken cancellationToken = default);
  Task<Player?> GetPlayerAsync(Nickname nickname, bool store = true);
  IReadOnlyList<string> GetPlayerNicknamesInBigQuadrantOf(Player player);
  List<Player> GetPlayersInQuadrant(uint quadrantIndex);
  Player UpdatePlayerQuadrant(Player player, uint? quadrantIndex);
  Player ClearLogoutTime(Player player);
  void ClearPlayer(Player player);
  void SetLogoutTime(Player player);

}
