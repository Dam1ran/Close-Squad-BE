using CS.Api.Support.Models.Abstractions;

namespace CS.Api.Support.Models;
public class ClientStatistics : StatisticsEntity {
  public ClientStatistics(int numberOfRequests) {
    if (numberOfRequests > 0) {
      NumberOfRequests = numberOfRequests;
    }
  }
  protected ClientStatistics() {}
}
