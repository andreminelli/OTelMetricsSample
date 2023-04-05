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
