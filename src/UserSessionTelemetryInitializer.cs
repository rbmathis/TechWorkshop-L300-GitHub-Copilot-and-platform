using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace ZavaStorefront
{
    // Adds user and session identifiers to all telemetry items
    public class UserSessionTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserSessionTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return; // No HTTP context available (e.g., background telemetry)
            }

            var userId = httpContext.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity?.Name
                : "anonymous";

            // Session may not be configured or available for this request; guard access
            var sessionFeature = httpContext.Features.Get<ISessionFeature>();
            var sessionId = sessionFeature?.Session?.Id ?? "no-session";

            telemetry.Context.User.Id = userId ?? "anonymous";
            telemetry.Context.Session.Id = sessionId;
        }
    }
}