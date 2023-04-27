using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecast2Controller : ControllerBase
    {
        // These metrics are demonstration code. 
        // On production code you should apply encapsulation best practices, as needed.
        // For instance, a base Controller class, a Filter or a Middleware should be used instead.
        private static readonly Histogram<double> _requestsDuration = MetricSource.Meter.CreateHistogram<double>(
            "http.requests.duration",
            unit: "Seconds",
            description: "Duration of Requests received");

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                await Task.Delay(Random.Shared.Next(3000));
                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            }
            finally
            {
                stopWatch.Stop();
                _requestsDuration.Record(stopWatch.Elapsed.TotalSeconds,
                    new("http.verb", "GET"),
                    new("app.controller", "WeatherForecast"));
            }
        }
    }
}