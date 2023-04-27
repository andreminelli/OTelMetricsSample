using Microsoft.AspNetCore.Http;
using System.Diagnostics.Metrics;

namespace OTelMetricsSample.API.Tests
{
    [TestClass]
    public class MetricsMiddlewareTests
    {
        private const string TotalMetricName = "http-requests-total";
        private const string InflightMetricName = "http-requests-inflight";

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
                    if (instrument.Name == TotalMetricName)
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            _meterListener.SetMeasurementEventCallback<int>(
                (instrument, measurement, tags, state) =>
                {
                    receivedMeasurement = measurement;
                });
            _meterListener.Start();

            // Act
            await Parallel.ForEachAsync(
                Enumerable.Repeat(0, requests),
                async (_, _) => await _target.InvokeAsync(_httpContext));

            // Assert
            _meterListener.RecordObservableInstruments();
            receivedMeasurement.ShouldBe(requests);
        }

        [TestMethod]
        public async Task RegisterInflightMetric()
        {
            // Arrange
            var receivedMeasurement = -1;
            _meterListener = new MeterListener
            {
                InstrumentPublished = (instrument, listener) =>
                {
                    if (instrument.Name == InflightMetricName)
                    {
                        listener.EnableMeasurementEvents(instrument);
                    }
                }
            };
            _meterListener.SetMeasurementEventCallback<int>(
                (instrument, measurement, tags, state) =>
                {
                    receivedMeasurement = measurement;
                });
            _meterListener.Start();

            // Act
            //await Parallel.ForEachAsync(
            //    Enumerable.Repeat(0, requests),
            //    async (_, _) => await _target.InvokeAsync(_httpContext));

            //// Assert
            //_meterListener.RecordObservableInstruments();
            //receivedMeasurement.ShouldBe(requests);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _meterListener?.Dispose();
        }
    }
}