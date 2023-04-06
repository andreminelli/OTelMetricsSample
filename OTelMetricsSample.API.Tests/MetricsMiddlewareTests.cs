using Microsoft.AspNetCore.Http;
using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API.Tests
{
    [TestClass]
    public class MetricsMiddlewareTests
    {
        private MeterListener _meterListener;
        private RequestDelegate _next;
        private HttpContext _httpContext;
        private Fixture _fixture;
        private MetricsMiddleware _target;


        [TestInitialize]
        public void Setup()
        {
            _fixture = new Fixture();
            _fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

            _next = context => Task.CompletedTask;
            _httpContext = _fixture.Create<HttpContext>();
            _httpContext.Request.RouteValues.Add("1", "2");

            _target = new MetricsMiddleware(_next);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(13)]
        public async Task RegisterTotalMetric(int requests)
        {
            // Arrange
            var receivedMeasurement = -1;
            _meterListener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Meter.Name == MetricSource.MeterName)
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            _meterListener.SetMeasurementEventCallback<int>(
                (instrument, measurement, tags, state) =>
                {
                    if (instrument.Name == "http-requests-total") receivedMeasurement = measurement;
                });
            _meterListener.Start();

            // Act
            await Parallel.ForEachAsync(
                Enumerable.Repeat(0, requests),
                async (_, _) => await _target.InvokeAsync(_httpContext));

            // Assert
            _meterListener.RecordObservableInstruments();
            receivedMeasurement.ShouldBe(requests);
            _meterListener?.Dispose();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _meterListener?.Dispose();
        }
    }
}