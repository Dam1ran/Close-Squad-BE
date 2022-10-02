using CS.Core.Entities;
using CS.Core.ValueObjects;

namespace CS.Application.Persistence.Abstractions.Repositories;
public interface IPlayerRepository: IRepository {

  public Task<Player?> FindByNicknameAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<Player?> FindByNicknameWithCharactersAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<List<Character>> GetPlayerCharactersWithQuadrantAsync(Player player, CancellationToken cancellationToken);

}
