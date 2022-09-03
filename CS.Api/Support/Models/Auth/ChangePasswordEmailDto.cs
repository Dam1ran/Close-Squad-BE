using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;
public class ChangePasswordEmailDto {

  [Required]
  [MinLength(CS.Core.ValueObjects.Email.MinLength, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Email.MaxLength, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
  [RegularExpression(CS.Core.ValueObjects.Email.RegexPattern, ErrorMessage = "Wrong email format.")]
  public string Email { get; set; } = string.Empty;

  [Required]
  [MinLength(5, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(255, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Repeat Email field is not a valid e-mail address.")]
  [RegularExpression(@"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$", ErrorMessage = "Wrong email format.")]
  [Compare("Email",  ErrorMessage = "The Email and Repeat email fields do not match.")]
  public string RepeatEmail { get; set; } = string.Empty;

}
