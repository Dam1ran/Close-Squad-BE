using CS.Application.DataTransferObjects;

namespace CS.Application.Models;
public class UserManagerResponse {

  public bool Successful => !(ErrorDetails?.Count > 0);
  public Dictionary<string, string> ErrorDetails { get; } = new();
  public int? IntegerData { get; set; }
  public string? Token { get; set; }
  public IdentificationRefreshTokenDto? RefreshToken { get; set; }
  public static UserManagerResponse Succeeded() => new();

  public static UserManagerResponse Failed(string code, string description) {
    var response = new UserManagerResponse();

    response.ErrorDetails.Add(code, description);

    return response;
  }

  public void AddErrorDetails(string code, string description) => ErrorDetails.Add(code, description);

}
