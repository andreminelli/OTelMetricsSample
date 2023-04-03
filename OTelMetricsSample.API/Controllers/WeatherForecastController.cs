using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        public const string MeterName = "OTelMetricsSample.API";

        internal static readonly Meter Meter = new(MeterName);


        private static             
        readonly Counter<int> _requestsCounter = Meter.CreateCounter<int>(
            "http-requests",
            unit: "HTTP Requests",
            description: "Number of Requests received");

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;

        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _requestsCounter.Add(1,
                new("Verb", "GET"),
                new("Controller", "WeatherForecast"));

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}