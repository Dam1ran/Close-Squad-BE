using CS.Api.Services.Abstractions;
using CS.Api.Support.Models.Abstractions;
using CS.Application.Services.Abstractions;
using CS.Application.Utils;

namespace CS.Api.Services;
public class StatisticsCacheService<T> : IStatisticsCacheService<T> where T : StatisticsEntity {
  private readonly ICacheService _cacheService;
  public StatisticsCacheService(ICacheService cacheService) {
    _cacheService = Check.NotNull(cacheService, nameof(cacheService));
  }

  public async Task<StatisticsEntityWrapper<T>?> GetAsync(string key, CancellationToken cancellationToken = default) {
    return await _cacheService.GetAsync<StatisticsEntityWrapper<T>>(key, cancellationToken);
  }

  public async Task SetAsync(string key, StatisticsEntityWrapper<T> statisticsEntityWrapper, CancellationToken cancellationToken = default) {
    await _cacheService.SetAsync(key, statisticsEntityWrapper, absoluteExpiration: statisticsEntityWrapper.ExpiresAt, cancellationToken: cancellationToken);
  }
}
