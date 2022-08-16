using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class SendEmailOptions {
  public const string SendEmail = "SendEmail";

  [Required(AllowEmptyStrings = false)]
  public string FromEmail { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string FromName { get; set; } = string.Empty;
}
