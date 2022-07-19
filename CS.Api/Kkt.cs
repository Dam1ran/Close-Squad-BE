using System.ComponentModel.DataAnnotations;

namespace CS.Api;
public class Kkt {
  [Required]
  public string? Name { get; set; }
  public IEnumerable<Cartof> Cartoafi { get; set; } = new List<Cartof> ();
}

public class Cartof {
  public string Sort { get; set; } = string.Empty;
}