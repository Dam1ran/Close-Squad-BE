using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class CaptchaOptions {
  public const string Captcha = "Captcha";

  [Required]
  [Range(1, 8)]
  public int MinLength { get; set; }

  [Required]
  [Range(1, 16)]
  public int MaxLength { get; set; }

  [Required(AllowEmptyStrings = false)]
  public string AllowedCharacters { get; set; } = string.Empty;

  [Required]
  [Range(30, 200)]
  public int HeightPx { get; set; }

  [Required]
  [Range(60, 400)]
  public int WidthPx { get; set; }

  [Required(AllowEmptyStrings = false)]
  [MinLength(7)]
  [MaxLength(7)]
  public string BackgroundHexColor { get; set; } = string.Empty;
  [MinLength(7)]
  [MaxLength(7)]
  public string ForegroundHexColor { get; set; } = string.Empty;
}
