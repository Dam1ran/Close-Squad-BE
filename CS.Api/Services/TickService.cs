using CS.Core.Services.Interfaces;

namespace CS.Api.Services;
public class TickService : ITickService
{
  public event EventHandler? on_100ms_tick;
  public event EventHandler? on_200ms_tick;
  public event EventHandler? on_500ms_tick;
  public event EventHandler? on_1000ms_tick;

  private readonly PeriodicTimer _100ms_timer = new(TimeSpan.FromMilliseconds(100));

  public TickService() {
    new Thread(Ticker).Start();
  }

  private async void Ticker() {
    byte counter = 0;
    while (await _100ms_timer.WaitForNextTickAsync()) {
      counter++;
      if (counter % 1 == 0) {
        on_100ms_tick?.Invoke(this, EventArgs.Empty);
      }
      if (counter % 2 == 0) {
        on_200ms_tick?.Invoke(this, EventArgs.Empty);
      }
      if (counter % 5 == 0) {
        on_500ms_tick?.Invoke(this, EventArgs.Empty);
      }
      if (counter % 10 == 0) {
        on_1000ms_tick?.Invoke(this, EventArgs.Empty);
        counter = 0;
      }
    };
  }

}
