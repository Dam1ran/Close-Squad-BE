using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;

public class ConfirmEmailDto {
  [Required]
  public string Guid { get; set; } = string.Empty;
  [Required]
  public string Token { get; set; } = string.Empty;
}