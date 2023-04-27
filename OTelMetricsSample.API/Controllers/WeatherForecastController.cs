using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        // These metrics are demonstration code. 
        // On production code you should apply encapsulation best practices, as needed.
        // For instance, a base Controller class, a Filter or a Middleware should be used instead.
        private static readonly Counter<int> _requestsCounter = MetricSource.Meter.CreateCounter<int>(
            "http.requests.count",
            unit: "HTTP Requests",
            description: "Number of Requests received");

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            _requestsCounter.Add(1,
                new("http.verb", "GET"),
                new("app.controller", "WeatherForecast"));

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