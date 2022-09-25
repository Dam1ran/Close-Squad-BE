using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IPlayerService {

  Task<Player?> GetPlayer(Nickname nickname, bool store = true, CancellationToken cancellationToken = default);
  Task<IReadOnlyList<string>> GetPlayerNicknamesInBigQuadrantOf(Player player, CancellationToken cancellationToken = default);
  Task<IEnumerable<Player>> GetPlayersInQuadrantOf(Player player, CancellationToken cancellationToken = default);
  public void RemovePlayer(Nickname nickname);

}
