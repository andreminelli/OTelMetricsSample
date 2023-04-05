using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly ObservableCounter<int> _requestsInflightCounter = MetricSource.Meter.CreateObservableCounter<int>(
            "http-requests-inflight",
            ObserveInflighRequests,
            unit: "HTTP Requests",
            description: "Number of Requests being served");

        private static int _inflightRequests = 0;

        private static int ObserveInflighRequests()
        {
            return _inflightRequests;
        }

        public MetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Interlocked.Increment(ref _inflightRequests);

            await _next(context);

            Interlocked.Decrement(ref _inflightRequests);
        }
    }
}
