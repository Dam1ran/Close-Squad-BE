using CS.Application.Persistence.Abstractions;
using CS.Application.Persistence.Abstractions.Repositories;
using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CS.Application.Persistence.Repositories;
public class CsUserRepository : Repository, ICsUserRepository {
  public CsUserRepository(IContext context) : base(context) {}

  public async Task AddAsync(CsUser csUser, CancellationToken cancellationToken) =>
    await _context.CsUsers.AddAsync(csUser, cancellationToken);

  public void Update(CsUser csUser) => _context.CsUsers.Update(csUser);

  public async Task<bool> AnyByNicknameAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname).AsNoTracking().AnyAsync(cancellationToken);

  public async Task<CsUser?> FindByNicknameWithVerificationAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname).Include(csu => csu.Verification).SingleOrDefaultAsync(cancellationToken);

  public async Task<CsUser?> FindByNicknameWithVerificationAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken) =>
    await ByNickname(nickname).Include(csu => csu.Verification).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

  public async Task<bool> AnyByEmailAsNoTrackingAsync(Email email, CancellationToken cancellationToken) =>
    await ByEmail(email).AsNoTracking().AnyAsync(cancellationToken);

  public async Task<CsUser?> FindByEmailAsync(Email email, CancellationToken cancellationToken) =>
    await ByEmail(email).SingleOrDefaultAsync(cancellationToken);

  public async Task<CsUser?> FindByEmailAsNoTrackingAsync(Email email, CancellationToken cancellationToken) =>
    await ByEmail(email).AsNoTracking().SingleOrDefaultAsync(cancellationToken);

  private IQueryable<CsUser> ByNickname(Nickname nickname) =>
    _context.CsUsers.Where(csu => csu.Nickname.ValueLowerCase == nickname.ValueLowerCase);

  private IQueryable<CsUser> ByEmail(Email email) =>
    _context.CsUsers
      .Include(csu => csu.Verification)
      .Where(csu => csu.Verification.Email.ValueLowerCase == email.ValueLowerCase);

  public async Task<CsUser?> FindByEmailWithAuthAsync(Email email, CancellationToken cancellationToken) =>
    await ByEmail(email)
      .Include(csu => csu.Identification)
      .Include(csu => csu.Identification.IdentificationPassword)
      .Include(csu => csu.Identification.IdentificationRefreshToken)
      .SingleOrDefaultAsync(cancellationToken);

  public async Task<CsUser?> FindByEmailWithIdentificationAsNoTrackingAsync(Email email, CancellationToken cancellationToken) =>
    await ByEmail(email)
      .Include(csu => csu.Identification)
      .SingleOrDefaultAsync(cancellationToken);

}
