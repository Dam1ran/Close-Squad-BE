using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;
public class UserRegisterDto {

  [Required]
  [MinLength(CS.Core.ValueObjects.Nickname.MinLength, ErrorMessage = "Minimum 4 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Nickname.MaxLength, ErrorMessage = "Maximum 20 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Nickname.RegexPattern, ErrorMessage = "The Nickname field accepts only letters, numbers and one underscore in between.")]
  public string Nickname { get; set; } = string.Empty;

  [Required]
  [MinLength(CS.Core.ValueObjects.Email.MinLength, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Email.MaxLength, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
  [RegularExpression(CS.Core.ValueObjects.Email.RegexPattern, ErrorMessage = "Wrong email format.")]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MinLength(CS.Core.ValueObjects.Email.MinLength, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Email.MaxLength, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Repeat Email field is not a valid e-mail address.")]
  [RegularExpression(CS.Core.ValueObjects.Email.RegexPattern, ErrorMessage = "Wrong email format.")]
  [Compare("Email",  ErrorMessage = "The Email and Repeat email fields do not match.")]
  public string RepeatEmail { get; set; } = string.Empty;

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
