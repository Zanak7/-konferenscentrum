using System.Security.Claims;
namespace KonferenscentrumVast;

// Middleware that logs every request
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Called for every HTTP request
    public async Task Invoke(HttpContext ctx)
    {
        // Starts a stopwatch to count the duration of the request
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Runs the code just before the client recieves a response
        ctx.Response.OnStarting(() =>
        {
            // If the user is logged in, take the users Id, otherwise log as "anonymous"
            var userId = ctx.User?.Identity?.IsAuthenticated == true
                ? (ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous")
                : "anonymous";

            // Try to get the id from the route (for example /api/customers/5)
            ctx.Request.RouteValues.TryGetValue("id", out var id);

            // Write an audit log
            _logger.LogInformation(
                "AUDIT UserId={UserId} Method={Method} Route={Path} Id={Id} Status={Status} Ms={Ms}",
                userId,                                // Who
                ctx.Request.Method,                  // HTTP method (GET/POST/DELETE etc.)
                ctx.Request.Path.Value,              // Route/path
                id,                                  // Id of the route/path
                ctx.Response.StatusCode,             // Final HTTP status code
                sw.Elapsed.TotalMilliseconds         // Duration in milliseconds
            );

            return Task.CompletedTask; // The log is completed
        });

        await _next(ctx); // Call the next middleware / controller
    }
}
