using CS.Application.Models.Dialog;

public class ServerDialog {
  public DialogType? DialogType { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public bool? Modal { get; set; }
  public bool? CanBeClosed { get; set; }
  public bool? CanBePaused { get; set; }
  public string? Icon { get; set; }
  public string? IconDescription { get; set; }
  public int? DurationMilliseconds { get; set; }

}
