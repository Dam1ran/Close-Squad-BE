using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;
public class UserLoginDto {
  [Required]
  [MinLength(CS.Core.ValueObjects.Email.MinLength, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Email.MaxLength, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
  [RegularExpression(CS.Core.ValueObjects.Email.RegexPattern, ErrorMessage = "Wrong email format.")]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MinLength(CS.Core.ValueObjects.Password.MinLength, ErrorMessage = "Minimum 8 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Password.MaxLength, ErrorMessage = "Maximum 64 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Password.RegexPattern, ErrorMessage = "The field must contain uppercase/lowercase letter, digit and non alphanumeric character.")]
  public string Password { get; set; } = string.Empty;

}
