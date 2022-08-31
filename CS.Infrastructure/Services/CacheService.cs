using System.Text.Json;
using CS.Application.Services.Abstractions;
using CS.Application.Support.Utils;
using Microsoft.Extensions.Caching.Distributed;

namespace CS.Infrastructure.Services;
public class CacheService : ICacheService
{
  private readonly IDistributedCache _distributedCache;

  public CacheService(IDistributedCache distributedCache) {
    _distributedCache = Check.NotNull(distributedCache, nameof(distributedCache));
  }

  private string ComposeKey(string groupKey, string key) =>
    string.Concat(groupKey, "_[", key, "]");

  public async Task<string?> GetStringAsync(string groupKey, string key, CancellationToken cancellationToken = default) =>
    await _distributedCache.GetStringAsync(ComposeKey(groupKey, key), cancellationToken);

  public async Task<T?> GetAsync<T>(string groupKey, string key, CancellationToken cancellationToken = default) {

    var str = await _distributedCache.GetStringAsync(ComposeKey(groupKey, key), cancellationToken);
    if (string.IsNullOrWhiteSpace(str)) {
      return default;
    }

    return JsonSerializer.Deserialize<T>(str);
  }

  public Task SetStringAsync(
    string groupKey,
    string key,
    string value,
    DateTimeOffset? absoluteExpiration = null,
    TimeSpan? absoluteExpirationRelativeToNow = null,
    TimeSpan? slidingExpiration = null,
    CancellationToken cancellationToken = default)
  {
    Check.NotNullOrWhiteSpace(key, nameof(key));
    Check.NotNullOrWhiteSpace(value, nameof(value));

    return _distributedCache.SetStringAsync(
      ComposeKey(groupKey, key),
      value,
      new DistributedCacheEntryOptions {
        AbsoluteExpiration = absoluteExpiration,
        AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
        SlidingExpiration = slidingExpiration
      },
      cancellationToken);
  }

  public Task SetAsync<T>(
    string groupKey,
    string key,
    T value,
    DateTimeOffset? absoluteExpiration = null,
    TimeSpan? absoluteExpirationRelativeToNow = null,
    TimeSpan? slidingExpiration = null,
    CancellationToken cancellationToken = default)
  {
    Check.NotNull(value, nameof(value));

    return SetStringAsync(groupKey, key, JsonSerializer.Serialize(value), absoluteExpiration, absoluteExpirationRelativeToNow, slidingExpiration, cancellationToken);
  }

  public Task RemoveAsync(string groupKey, string key, CancellationToken cancellationToken = default) =>
    _distributedCache.RemoveAsync(ComposeKey(groupKey, key), cancellationToken);

}
