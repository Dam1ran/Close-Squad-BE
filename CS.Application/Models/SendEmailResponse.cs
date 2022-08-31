namespace CS.Application.Models;
public class SendEmailResponse {
  public bool Successful => !(Errors?.Count > 0);
  public List<string> Errors { get; set; } = new();
  public static SendEmailResponse Failed(string error) {
    var response = new SendEmailResponse();

    response.Errors.Add(error);

    return response;

  }
}
