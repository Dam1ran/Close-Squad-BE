using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IPlayerService {

  Task<Player?> GetPlayerAsync(Nickname nickname, bool store = true, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<string>> GetPlayerNicknamesInBigQuadrantOf(Player player, CancellationToken cancellationToken = default);
  List<Player> GetPlayersInQuadrant(Quadrant quadrant);
  public void ClearPlayer(Nickname nickname);
  Task<Player?> UpdatePlayerQuadrant(Nickname nickname, Quadrant? quadrant, CancellationToken cancellationToken = default);

}
