using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Core.Entities;
using CS.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CS.Application.Persistence.Repositories;
public class PlayerRepository : Repository, IPlayerRepository {

  public PlayerRepository(IContext context) : base(context) {}

  private IQueryable<Player> ByNickname(Nickname nickname) =>
    _context.Players.Where(p => p.Nickname.ValueLowerCase == nickname.ValueLowerCase);

  public async Task<Player?> FindByNicknameAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

  public async Task<Player?> FindByNicknameWithCharactersAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname)
      .Include(p => p.Characters)
      .SingleOrDefaultAsync(cancellationToken);

  public Task<List<Character>> GetPlayerCharactersWithQuadrantAsync(Player player, CancellationToken cancellationToken) =>
    _context.Characters
      .Where(c => c.Player.Nickname.ValueLowerCase == player.Nickname.ValueLowerCase)
      .Include(c => c.Quadrant)
      .OrderByDescending(c => c.Level)
        .ThenByDescending(c => c.XP)
      .ToListAsync(cancellationToken);

}
