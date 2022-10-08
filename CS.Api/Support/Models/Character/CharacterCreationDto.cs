using System.ComponentModel.DataAnnotations;
using CS.Api.Support.Attributes;
using CS.Core.Enums;

namespace CS.Api.Support.Models.Character;
public class CharacterCreationDto {

  [Required]
  [MinLength(CS.Core.ValueObjects.Nickname.MinLength, ErrorMessage = "Minimum 4 characters long.")]
  [MaxLength(CS.Core.ValueObjects.Nickname.MaxLength, ErrorMessage = "Maximum 20 characters long.")]
  [RegularExpression(CS.Core.ValueObjects.Nickname.RegexPattern, ErrorMessage = "The Nickname field accepts only letters, numbers and one underscore in between.")]
  public string Nickname { get; set; } = string.Empty;

  [Required]
  [Enum(typeof(CharacterClass))]
  public CharacterClass CharacterClass { get; set; }

  [Required]
  [Range(0, 100)]
  public int Gender { get; set; }

}
