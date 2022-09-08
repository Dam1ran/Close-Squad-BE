using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class JwtOptions {
  public const string Jwt = "Jwt";

  [Required(AllowEmptyStrings = false)]
  public string Secret { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string ValidIssuer { get; set; } = string.Empty;

  [Required(AllowEmptyStrings = false)]
  public string ValidAudience { get; set; } = string.Empty;

  [Required]
  [Range(1, 60)]
  public int ExpiresInMinutes { get; set; }

  [Required]
  [Range(0, 300)]
  public int ClockSkewSeconds { get; set; }

}
