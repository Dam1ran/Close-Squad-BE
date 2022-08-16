using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class SendGridOptions {
  public const string SendGrid = "SendGrid";

  [Required(AllowEmptyStrings = false)]
  public string Key { get; set; } = string.Empty;
}
