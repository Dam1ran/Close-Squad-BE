using CS.Core.Entities.Auth;

namespace CS.Application.Services.Abstractions;
public interface IUserTokenService {

  Task<IdentificationRefreshToken> CreateAndCacheIrt(CsUser csUser, CancellationToken cancellationToken);
  Task<string> CacheAndGetIdentificationToken(CsUser csUser, CancellationToken cancellationToken);

}
