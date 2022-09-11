using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;

public class SessionIdDto {

  [Required(AllowEmptyStrings = false)]
  [StringLength(36)]
  public string SessionId { get; set; } = string.Empty;

}
