using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;

public class ConfirmEmailDto {

  [Required(AllowEmptyStrings = false)]
  public string Guid { get; set; } = string.Empty;

}
