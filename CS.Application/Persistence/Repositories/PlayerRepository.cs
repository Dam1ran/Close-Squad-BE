using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Core.Entities;
using CS.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CS.Application.Persistence.Repositories;
public class PlayerRepository : Repository, IPlayerRepository {

  public PlayerRepository(IContext context) : base(context) {}

  public async Task AddAsync(Player player, CancellationToken cancellationToken) =>
    await _context.Players.AddAsync(player, cancellationToken);

  private IQueryable<Player> ByNickname(Nickname nickname) =>
    _context.Players.Where(p => p.Nickname.ValueLowerCase == nickname.ValueLowerCase);

  public async Task<Player?> FindByNicknameAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken = default) =>
    await ByNickname(nickname).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

  public async Task<Player?> FindByNicknameWithCharactersAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname)
      .Include(p => p.Characters)
      .AsNoTracking()
      .SingleOrDefaultAsync(cancellationToken);

  public Task<List<Character>> GetPlayerCharactersAsNoTrackingAsync(long playerId, CancellationToken cancellationToken) =>
    _context.Characters
      .Where(c => c.PlayerId == playerId)
      .OrderByDescending(c => c.Level)
        // .ThenByDescending(c => c.XP)
      .AsNoTracking()
      .ToListAsync(cancellationToken);

}
