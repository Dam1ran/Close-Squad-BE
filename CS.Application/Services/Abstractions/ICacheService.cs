namespace CS.Application.Services.Abstractions;
public interface ICacheService {
  Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);
  Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
  Task SetStringAsync(string key, string value, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
  Task SetAsync<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
  Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
