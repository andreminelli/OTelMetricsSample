using OTelMetricsSample.API;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics();

    Metrics.ConfigureMeterAdapter(options =>
    {
        options.ResolveHistogramBuckets = instrument =>
        { 
            switch(instrument.Name)
            {
                //case "http-requests-duration":
                //    return new double[] { 0.001, 0.010, 0.050, 0.100, 0.200, 0.500, 1, 2, 5 };
                case "http-requests-duration":
                    return Histogram.ExponentialBuckets(0.001, 2, 14);
                default:
                    return MeterAdapterOptions.DefaultHistogramBuckets;
            }
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

app.MapControllers();

app.Run();
