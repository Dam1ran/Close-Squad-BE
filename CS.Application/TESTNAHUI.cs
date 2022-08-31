using CS.Core.Exceptions;
using CS.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
namespace CS.Application;
public class TESTNAHUI {
  private readonly ILogger<TESTNAHUI> _logger;
  private readonly ITickService _tickService;
  private int counter = 0;

  public TESTNAHUI(ILogger<TESTNAHUI> logger, ITickService tickService) {
    _logger = logger;
    _tickService = tickService;
    test();
  }

  private void test() {
    _tickService.on_1000ms_tick += update;
  }
  public void gg() {
    throw new NotFoundException("SUK");
  }
  private void update(object? sender, EventArgs e) {
    counter++;
    // _logger.LogWarning("UPDATED");
    if (counter > 10) {
      _tickService.on_1000ms_tick -= update;
      _logger.LogWarning("OTPISKA NAHUI");
    }
  }
}