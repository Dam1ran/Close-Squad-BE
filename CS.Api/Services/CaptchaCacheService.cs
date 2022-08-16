using CS.Api.Services.Abstractions;
using CS.Api.Support.Models;
using CS.Application.Services.Abstractions;
using CS.Application.Utils;

namespace CS.Api.Services;
public class CaptchaCacheService : ICaptchaCacheService {
  private readonly ICacheService _cacheService;
  private const int AbsoluteExpirationRelativeToNowMinutes = 1;
  public CaptchaCacheService(ICacheService cacheService) {
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task<CachedCaptcha?> GetAsync(string key, CancellationToken cancellationToken) {
    return await _cacheService.GetAsync<CachedCaptcha>(key, cancellationToken);
  }

  public async Task SetAsync(string key, CachedCaptcha cachedCaptcha, CancellationToken cancellationToken) {
    Check.NotNullOrWhiteSpace(key, nameof(key));
    Check.NotNull(cachedCaptcha, nameof(cachedCaptcha));

    await _cacheService.SetAsync<CachedCaptcha>(
      key,
      cachedCaptcha,
      absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(AbsoluteExpirationRelativeToNowMinutes),
      cancellationToken: cancellationToken);

  }
  public async Task RemoveAsync(string key, CancellationToken cancellationToken) {
    Check.NotNullOrWhiteSpace(key, nameof(key));

    await _cacheService.RemoveAsync(key, cancellationToken);
  }
}
