using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API
{
    public class MetricsMiddleware
    {
        // These metrics are demonstration code. 
        private static readonly ObservableGauge<int> _requestsInflightGauge =
            MetricSource.Meter.CreateObservableGauge<int>(
                "http-requests-inflight",
                ObserveInflighRequests,
                unit: "HTTP Requests",
                description: "Number of Requests being served");

        private static readonly ObservableCounter<int> _requestsTotalCounter =
            MetricSource.Meter.CreateObservableCounter<int>(
                "http-requests-total",
                ObserveTotalRequests,
                unit: "HTTP Requests",
                description: "Number of Requests being served");

        private static int _inflightRequests = 0;
        private static int _totalRequests = 0;

        private readonly RequestDelegate _next;


        private static int ObserveInflighRequests()
        {
            return _inflightRequests;
        }

        private static int ObserveTotalRequests()
        {
            return _totalRequests;
        }

        public MetricsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSkipMetrics(context))
            {
                await _next(context);
                return;
            }

            Interlocked.Increment(ref _totalRequests);
            Interlocked.Increment(ref _inflightRequests);

            await _next(context);

            Interlocked.Decrement(ref _inflightRequests);
        }

        private static bool ShouldSkipMetrics(HttpContext context)
            => !IsControllerRequest(context);

        private static bool IsControllerRequest(HttpContext context) 
            => context.Request.RouteValues.Any();
    }
}
