namespace KonferenscentrumVast;

//Middleware that logs every request
public class AuditMiddleware
{
    private readonly RequestDelegate _next; 
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    //Called for every HTTP request
    public async Task Invoke(HttpContext ctx)
    {
        var t0 = DateTime.UtcNow; //Start time of the request
        await _next(ctx); // Call the next middleware / controller

        // If the user is logged in, use their name, otherwise "anonymous
        var user = ctx.User?.Identity?.IsAuthenticated == true
            ? ctx.User.Identity!.Name
            : "anonymous";

        // Try to get the id from the route (for example /api/customers/5)
        ctx.Request.RouteValues.TryGetValue("id", out var id);

        //Write an audit log
        _logger.LogInformation(
            "AUDIT User={User} Method={Method} Route={Path} Id={Id} Status={Status} Ms={Ms}",
            user,                               // Who
            ctx.Request.Method,                 // HTTP method (GET/POST/DELETE etc.)
            ctx.Request.Path.Value,             // Route/path
            id,                                 // Id of the route/path
            ctx.Response.StatusCode,            // HTTP status code (200/400 etc.)
            (DateTime.UtcNow - t0).TotalMilliseconds // Duration in miliseconds
        );
    }

}