using CS.Api.Support.Models.Abstractions;

namespace CS.Api.Services.Abstractions;
public interface IStatisticsCacheService<T> where T : StatisticsEntity {
  Task<StatisticsEntityWrapper<T>?> GetAsync(string key, CancellationToken cancellationToken = default);
  Task SetAsync(string key, StatisticsEntityWrapper<T> statisticsEntity, CancellationToken cancellationToken = default);
}
