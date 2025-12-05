using Microsoft.ApplicationInsights.DataContracts;

namespace ZavaStorefront.Services
{
    /// <summary>
    /// Interface for Application Insights telemetry client to enable mocking in tests.
    /// </summary>
    public interface ITelemetryClient
    {
        /// <summary>
        /// Track a custom event.
        /// </summary>
        void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Track an exception.
        /// </summary>
        void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);

        /// <summary>
        /// Track a trace message.
        /// </summary>
        void TrackTrace(string message, SeverityLevel severityLevel = SeverityLevel.Information, IDictionary<string, string>? properties = null);
    }
}
