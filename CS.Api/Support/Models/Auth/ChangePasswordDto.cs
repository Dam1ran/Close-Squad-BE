using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;
public class ChangePasswordDto {

  [Required(AllowEmptyStrings = false)]
  public string Guid { get; set; } = string.Empty;

  [Required]
  [MinLength(CS.Core.ValueObjects.Password.MinLength, ErrorMessage = "Minimum 8 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Password.MaxLength, ErrorMessage = "Maximum 64 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Password.RegexPattern, ErrorMessage = "The field must contain uppercase/lowercase letter, digit and non alphanumeric character.")]
  public string Password { get; set; } = string.Empty;

  [Required]
  [MinLength(CS.Core.ValueObjects.Password.MinLength, ErrorMessage = "Minimum 8 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Password.MaxLength, ErrorMessage = "Maximum 64 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Password.RegexPattern, ErrorMessage = "The field must contain uppercase/lowercase letter, digit and non alphanumeric character.")]
  [Compare("Password",  ErrorMessage = "The Password and Repeat Password fields do not match.")]
  public string RepeatPassword { get; set; } = string.Empty;

}
