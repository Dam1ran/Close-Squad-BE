namespace CS.Infrastructure.Models;
public class EmailDetails {
  public string FromName { get; set; } = string.Empty;
  public string FromEmail { get; set; } = string.Empty;
  public string ToName { get; set; } = string.Empty;
  public string ToEmail { get; set; } = string.Empty;
  public string Subject { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public bool IsHTML { get; set; }
}