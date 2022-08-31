using System.ComponentModel.DataAnnotations;

namespace CS.Application.Options;
public class RefreshTokenOptions {
  public const string RefreshToken = "RefreshToken";

  [Required]
  [Range(1, 10080)]
  public int ExpiresInMinutes { get; set; }

}
