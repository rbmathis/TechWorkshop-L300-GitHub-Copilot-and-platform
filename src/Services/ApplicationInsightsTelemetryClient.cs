using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace ZavaStorefront.Services
{
    /// <summary>
    /// Wrapper for Application Insights TelemetryClient to enable dependency injection and testing.
    /// </summary>
    public class ApplicationInsightsTelemetryClient : ITelemetryClient
    {
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsTelemetryClient(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        public void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null)
        {
            _telemetryClient.TrackException(exception, properties, metrics);
        }

        public void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, IDictionary<string, string>? properties = null)
        {
            _telemetryClient.TrackTrace(message, severityLevel, properties);
        }
    }
}
