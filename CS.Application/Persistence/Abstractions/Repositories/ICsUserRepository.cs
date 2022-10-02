using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;

namespace CS.Application.Persistence.Abstractions.Repositories;
public interface ICsUserRepository: IRepository {

  public Task<bool> AnyByNicknameAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<CsUser?> FindByNicknameWithVerificationAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<CsUser?> FindByNicknameWithVerificationAndIdentificationPasswordAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<CsUser?> FindByNicknameWithVerificationAndIdentificationRefreshTokenAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<CsUser?> FindByNicknameWithVerificationAndIdentificationRefreshTokenAsync(Nickname nickname, CancellationToken cancellationToken);
  public Task<CsUser?> FindByNicknameWithVerificationAsNoTrackingAsync(Nickname nickname, CancellationToken cancellationToken);

  public Task<bool> AnyByEmailAsNoTrackingAsync(Email email, CancellationToken cancellationToken);
  public Task<CsUser?> FindByEmailAsNoTrackingAsync(Email email, CancellationToken cancellationToken);
  public Task<CsUser?> FindByEmailAsync(Email email, CancellationToken cancellationToken);
  public Task<CsUser?> FindByEmailWithAuthAsync(Email email, CancellationToken cancellationToken);

  public Task AddAsync(CsUser csUser, CancellationToken cancellationToken);
  public void Update(CsUser csUser);

}
