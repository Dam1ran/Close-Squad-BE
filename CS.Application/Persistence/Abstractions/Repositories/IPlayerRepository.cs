using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Persistence.Abstractions.Repositories;
public interface IPlayerRepository: IRepository {

  public Task AddAsync(Player player, CancellationToken cancellationToken);
  public Task<Player?> FindByNicknameAsNoTrackingAsync(Nickname playerNickname, CancellationToken cancellationToken = default);
  public Task<Player?> FindByNicknameWithCharactersAsNoTrackingAsync(Nickname playerNickname, CancellationToken cancellationToken);
  public Task<List<Character>> GetPlayerCharactersAsNoTrackingAsync(long playerId, CancellationToken cancellationToken);

}
