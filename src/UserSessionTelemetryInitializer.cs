using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

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
            var userId = httpContext?.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity?.Name
                : "anonymous";
            var sessionId = httpContext?.Session?.Id ?? "no-session";

            telemetry.Context.User.Id = userId ?? "anonymous";
            telemetry.Context.Session.Id = sessionId;
        }
    }
}