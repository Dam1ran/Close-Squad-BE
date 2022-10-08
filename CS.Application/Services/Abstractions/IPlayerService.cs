using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IPlayerService {

  Task<Player> GetOrCreatePlayerAsync(Nickname playerNickname, CancellationToken cancellationToken = default);
  Task<Player?> GetPlayerAsync(Nickname nickname, bool store = true);
  IReadOnlyList<string> GetPlayerNicknamesInBigQuadrantOf(Player player);
  List<Player> GetPlayersInQuadrant(uint quadrantIndex);
  Player UpdatePlayerQuadrant(Player player, uint? quadrantIndex);
  Player ClearLogoutTime(Player player);
  void SetLogoutTime(Player player);

}
