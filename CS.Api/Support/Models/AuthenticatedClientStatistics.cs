using CS.Api.Support.Models.Abstractions;

namespace CS.Api.Support.Models;
public class AuthenticatedClientStatistics : StatisticsEntity {
  public AuthenticatedClientStatistics(int numberOfRequests) {
    if (numberOfRequests > 0) {
      NumberOfRequests = numberOfRequests;
    }
  }
  protected AuthenticatedClientStatistics() {}
}
