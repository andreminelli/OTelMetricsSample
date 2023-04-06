using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API
{
    public static class MetricSource
    {
        public const string MeterName = "OTelMetricsSample.API";

        internal static Meter Meter { get; } = new(MeterName);
    }
}
