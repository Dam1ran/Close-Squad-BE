using CS.Core.Extensions;

namespace CS.Api.Support.Models.Abstractions;
public class StatisticsEntity {
  protected StatisticsEntity() {}
  public StatisticsEntity(int numberOfRequests, DateTimeOffset expiresAt) {
    if (!expiresAt.IsInFuture()) {
      throw new ArgumentException($"{nameof(expiresAt)} cannot be in the past");
    }
    if (numberOfRequests > 0) {
      NumberOfRequests = numberOfRequests;
    }

    ExpiresAt = expiresAt;
  }

  public int NumberOfRequests { get; set; }
  public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.UtcNow;

}
