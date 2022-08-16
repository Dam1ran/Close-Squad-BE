using System.Text.Json;
using CS.Application.Services.Abstractions;
using CS.Application.Utils;
using Microsoft.Extensions.Caching.Distributed;

namespace CS.Infrastructure.Services;
public class CacheService : ICacheService
{
  private readonly IDistributedCache _distributedCache;

  public CacheService(IDistributedCache distributedCache) {
    _distributedCache = Check.NotNull(distributedCache, nameof(distributedCache));
  }

  public Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default) {
    Check.NotNullOrWhiteSpace(key, nameof(key));
    return _distributedCache.GetStringAsync(key, cancellationToken);
  }

  public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) {
    Check.NotNullOrWhiteSpace(key, nameof(key));

    var str = await _distributedCache.GetStringAsync(key, cancellationToken);
    if (string.IsNullOrWhiteSpace(str)) {
      return default;
    }

    return JsonSerializer.Deserialize<T>(str);
  }

  public Task SetStringAsync(string key,
  string value, DateTimeOffset? absoluteExpiration = null,
  TimeSpan? absoluteExpirationRelativeToNow = null,
  TimeSpan? slidingExpiration = null,
  CancellationToken cancellationToken = default)
  {
    Check.NotNullOrWhiteSpace(key, nameof(key));
    Check.NotNullOrWhiteSpace(value, nameof(value));

    return _distributedCache.SetStringAsync(
      key,
      value,
      new DistributedCacheEntryOptions {
        AbsoluteExpiration = absoluteExpiration,
        AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
        SlidingExpiration = slidingExpiration
      },
      cancellationToken);
  }

  public Task SetAsync<T>(
    string key,
    T value,
    DateTimeOffset? absoluteExpiration = null,
    TimeSpan? absoluteExpirationRelativeToNow = null,
    TimeSpan? slidingExpiration = null,
    CancellationToken cancellationToken = default)
  {
    Check.NotNullOrWhiteSpace(key, nameof(key));
    Check.NotNull(value, nameof(value));

    return SetStringAsync(key, JsonSerializer.Serialize(value), absoluteExpiration, absoluteExpirationRelativeToNow, slidingExpiration, cancellationToken);
  }

  public Task RemoveAsync(string key, CancellationToken cancellationToken = default) {
    Check.NotNullOrWhiteSpace(key, nameof(key));

    return _distributedCache.RemoveAsync(key, cancellationToken);
  }

}
