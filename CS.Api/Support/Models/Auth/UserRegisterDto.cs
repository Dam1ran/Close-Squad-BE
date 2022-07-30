using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;

public class UserRegisterDto {
  [Required]
  [MinLength(4, ErrorMessage = "Minimum 4 characters long.")]
  [MaxLength(20, ErrorMessage = "Maximum 20 characters long.")]
  [RegularExpression(@"^[A-Za-z0-9]{4,20}$", ErrorMessage = "The field accepts letters and numbers only.")]
  public string Nickname { get; set; } = string.Empty;
  [Required]
  [MinLength(5, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(255, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
  [RegularExpression(@"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$", ErrorMessage = "Wrong email format.")]
  public string Email { get; set; } = string.Empty;
  [Required]
  [MinLength(5, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(255, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Repeat Email field is not a valid e-mail address.")]
  [RegularExpression(@"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$", ErrorMessage = "Wrong email format.")]
  [Compare("Email",  ErrorMessage = "The Email and Repeat email fields do not match.")]
  public string RepeatEmail { get; set; } = string.Empty;
  [Required]
  [MinLength(8, ErrorMessage = "Minimum 8 characters long.")]
  [MaxLength(64, ErrorMessage = "Maximum 64 characters long.")]
  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,64}$", ErrorMessage = "The field must contain uppercase/lowercase letter, digit and non alphanumeric character.")]
  public string Password { get; set; } = string.Empty;
  [Required]
  [MinLength(8, ErrorMessage = "Minimum 8 characters long.")]
  [MaxLength(64, ErrorMessage = "Maximum 64 characters long.")]
  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,64}$", ErrorMessage = "The field must contain uppercase/lowercase letter, digit and non alphanumeric character.")]
  [Compare("Password",  ErrorMessage = "The Password and Repeat Password fields do not match.")]
  public string RepeatPassword { get; set; } = string.Empty;
}