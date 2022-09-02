using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class ExternalInfoOptions {
  public const string ExternalInfo = "ExternalInfo";

  [Required(AllowEmptyStrings = false)]
  public string WebSiteLink { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string ConfirmAddressLink { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string ChangePasswordLink { get; set; } = string.Empty;

}
