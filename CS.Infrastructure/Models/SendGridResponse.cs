namespace CS.Infrastructure.Models;
public class SendGridResponse {
  public List<SendGridResponseError> Errors { get; set; } = new();
}
