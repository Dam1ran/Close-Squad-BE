namespace CS.Infrastructure.Models;
public class SendGridResponseError {
  public string Message { get; set; } = string.Empty;
  public string Field { get; set; } = string.Empty;
  public string Help { get; set; } = string.Empty;
}