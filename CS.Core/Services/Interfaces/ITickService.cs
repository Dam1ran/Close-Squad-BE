namespace CS.Core.Services.Interfaces;

public interface ITickService {
  public event EventHandler on_100ms_tick;
  public event EventHandler? on_200ms_tick;
  public event EventHandler? on_500ms_tick;
  public event EventHandler? on_1000ms_tick;
}