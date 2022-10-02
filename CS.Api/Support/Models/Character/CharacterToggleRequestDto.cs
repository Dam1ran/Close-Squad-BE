using System.ComponentModel.DataAnnotations;

namespace CS.Api.Support.Models.Character;
public class CharacterToggleRequestDto {

  [Required]
  [MinLength(CS.Core.ValueObjects.Nickname.MinLength, ErrorMessage = "Minimum 4 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Nickname.MaxLength, ErrorMessage = "Maximum 20 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Nickname.RegexPattern, ErrorMessage = "The Nickname field accepts only letters, numbers and one underscore in between.")]
  public string Nickname { get; set; } = string.Empty;

}
