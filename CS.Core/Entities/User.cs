using CS.Core.Entities.Abstractions;

namespace CS.Core.Entities;

public class User : Entity {
  public string Email { get; set; } = string.Empty;
}