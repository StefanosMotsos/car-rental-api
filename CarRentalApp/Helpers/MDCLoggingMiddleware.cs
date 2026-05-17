using Serilog.Context;

namespace CarRentalApp.Helpers
{
    public class MDCLoggingMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var user = context.User.Identity?.Name ?? "Anonymous";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            using (LogContext.PushProperty("User", user))
            using (LogContext.PushProperty("IP", ip))
            {
                await next(context);
            }
        }
    }
}
