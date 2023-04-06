using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;

        // These metrics are demonstration code. 
        private readonly ObservableGauge<int> _requestsInflightGauge;
        private readonly ObservableCounter<int> _requestsTotalCounter;

        private int _inflightRequests = 0;
        private int _totalRequests = 0;

        public MetricsMiddleware(RequestDelegate next)
        {
            _next = next;

            _requestsInflightGauge = MetricSource.Meter.CreateObservableGauge<int>(
                "http-requests-inflight",
                ObserveInflighRequests,
                unit: "HTTP Requests",
                description: "Number of Requests being served");

            _requestsTotalCounter =
                MetricSource.Meter.CreateObservableCounter<int>(
                    "http-requests-total",
                    ObserveTotalRequests,
                    unit: "HTTP Requests",
                    description: "Number of Requests being served");
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

        private int ObserveInflighRequests() => _inflightRequests;

        private int ObserveTotalRequests() => _totalRequests;

    }
}
