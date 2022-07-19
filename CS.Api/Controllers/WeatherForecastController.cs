using CS.Application;
using Microsoft.AspNetCore.Mvc;

namespace CS.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly TESTNAHUI _tickService;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        TESTNAHUI tickService
        ) {
        _logger = logger;
        _tickService = tickService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task <ActionResult<IEnumerable<WeatherForecast>>> Get() {
        _logger.LogWarning(Request.Headers["refresh-token"]);
        // var ziu = await Task.FromResult(true);
        _tickService.gg();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
    // [HttpGet(Name = "GetWeatherForecast")]
    // public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
    // {
    //     _logger.LogWarning(Request.Headers["refresh-token"]);
    //     // var ziu = await Task.FromResult(true);
    //     _tickService.gg();
    //     return Enumerable.Range(1, 5).Select(index => new WeatherForecast
    //     {
    //         Date = DateTime.Now.AddDays(index),
    //         TemperatureC = Random.Shared.Next(-20, 55),
    //         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    //     })
    //     .ToArray();
    // }
}
