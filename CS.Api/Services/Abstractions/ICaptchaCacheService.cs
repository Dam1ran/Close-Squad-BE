using CS.Api.Support.Models;

namespace CS.Api.Services.Abstractions;
public interface ICaptchaCacheService {
  Task<CachedCaptcha?> GetAsync(string key, CancellationToken cancellationToken);
  Task SetAsync(string key, CachedCaptcha cachedCaptcha, CancellationToken cancellationToken);
  Task RemoveAsync(string key, CancellationToken cancellationToken);
}
