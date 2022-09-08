using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IUserTokenService {

  Task<IdentificationRefreshToken> CreateAndCacheIrtAsync(CsUser csUser, CancellationToken cancellationToken);
  Task<string> CreateAndCacheItAsync(Nickname nickname, string role, CancellationToken cancellationToken);

}
