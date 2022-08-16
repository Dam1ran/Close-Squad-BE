namespace CS.Api.Support.Models.Abstractions;
public class StatisticsEntityWrapper<T> where T : StatisticsEntity {
  public DateTimeOffset ExpiresAt { get; set; }
  public T? Entity { get; set; }
  public static StatisticsEntityWrapper<T> FromEntity(T entity, DateTimeOffset expiresAt) => new () { ExpiresAt = expiresAt, Entity = entity };
}
