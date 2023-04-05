
using Microsoft.AspNetCore.Mvc;

namespace OTelMetricsSample.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlowController : ControllerBase
    {
        [HttpGet]
        public async Task<string> GetAsync(int secondsDelay = 10)
        {
            secondsDelay = Request.QueryString.HasValue && int.TryParse(Request.QueryString.Value.TrimStart('?'), out int value) 
                ? value 
                : secondsDelay;

            await Task.Delay(TimeSpan.FromSeconds(secondsDelay));

            return $"You waited for {secondsDelay} seconds. Sorry...";
        }
    }
}