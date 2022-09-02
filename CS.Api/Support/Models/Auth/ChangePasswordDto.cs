using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Auth;
public class ChangePasswordDto {

  [Required]
  [MinLength(CS.Core.ValueObjects.Email.MinLength, ErrorMessage = "Minimum 5 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Email.MaxLength, ErrorMessage = "Maximum 255 characters long.")]
  [EmailAddress(ErrorMessage = "The Email field is not a valid e-mail address.")]
  [RegularExpression(CS.Core.ValueObjects.Email.RegexPattern, ErrorMessage = "Wrong email format.")]
  public string Email { get; set; } = string.Empty;

}
