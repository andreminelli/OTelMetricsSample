using OTelMetricsSample.API;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseAuthorization();

app.UseMiddleware<MetricsMiddleware>();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();

    Metrics.ConfigureMeterAdapter(options =>
    {
        options.ResolveHistogramBuckets = instrument =>
        {
            return instrument.Unit switch
            {
                "Seconds" => Histogram.ExponentialBuckets(0.001, 2, 14),
                "Bytes" => new double[] { 1, 1000, 2000, 5000, 10000, 20000, 50000, 85000 },
                _ => MeterAdapterOptions.DefaultHistogramBuckets,
            };
        };
        options.InstrumentFilterPredicate = instrument =>
        {
            return instrument.Meter.Name == MetricSource.MeterName;
        };
    });

    Metrics.SuppressDefaultMetrics(new SuppressDefaultMetricOptions
    {
        SuppressEventCounters = true,
        SuppressDebugMetrics = true
    });
});

app.Run();
