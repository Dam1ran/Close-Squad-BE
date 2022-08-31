namespace CS.Application.Services.Abstractions;
public interface ICacheService {

  Task<string?> GetStringAsync(string groupKey, string key, CancellationToken cancellationToken = default);
  Task<T?> GetAsync<T>(string groupKey, string key, CancellationToken cancellationToken = default);
  Task SetStringAsync(string groupKey, string key, string value, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
  Task SetAsync<T>(string groupKey, string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
  Task RemoveAsync(string groupKey, string key, CancellationToken cancellationToken = default);

}
