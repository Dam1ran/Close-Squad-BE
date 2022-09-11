using CS.Core.Entities.Auth;
using CS.Core.ValueObjects;

namespace CS.Application.Services.Abstractions;
public interface IUserTokenService {

  Task<IdentificationRefreshToken> CreateAndCacheIrtAsync(Nickname nickname, string role, string sessionIdValue, CancellationToken cancellationToken);
  Task<string> CreateAndCacheItAsync(Nickname nickname, string role, CancellationToken cancellationToken);

}
