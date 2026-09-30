namespace Skill_Loop.Api.Middlewares;

/// <summary>
/// Adds baseline security response headers.
///
/// <c>X-Content-Type-Options: nosniff</c> is the important one here: user-supplied HTML is
/// stored (course descriptions, email log bodies, uploaded material names). Without
/// nosniff a browser may sniff an uploaded file as HTML and execute it in the site's origin.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-Permitted-Cross-Domain-Policies"] = "none";
        headers["Content-Security-Policy"] = BuildContentSecurityPolicy(context);

        // Do not advertise the server stack.
        headers.Remove("X-Powered-By");

        return next(context);
    }

    /// <summary>
    /// Scalar's reference page bootstraps itself from an inline
    /// <c>&lt;script type="module"&gt;</c> block in the served HTML. A CSP of
    /// <c>default-src 'self'</c> does not imply <c>script-src 'unsafe-inline'</c>, so the
    /// browser blocks that inline block and the page renders as a blank page - the HTML
    /// still returns 200, which makes it look like a server problem when it is not.
    ///
    /// The fix is scoped rather than global. API responses are JSON, so relaxing
    /// script-src for them costs nothing; loosening it everywhere would undo the header
    /// for any endpoint that ever returns HTML. <c>script-src-elem 'unsafe-inline'</c> is
    /// used instead of the broader <c>script-src 'unsafe-inline'</c> because it leaves
    /// <c>eval</c> blocked, which is the part anyone actually wants off.
    /// </summary>
    private string BuildContentSecurityPolicy(HttpContext context)
    {
        var isScalarUi =
            environment.IsDevelopment() &&
            context.Request.Path.StartsWithSegments("/scalar");

        return isScalarUi
            ? "default-src 'self'; script-src 'self' 'unsafe-inline'; " +
              "style-src 'self' 'unsafe-inline'; frame-ancestors 'none'; base-uri 'self'; object-src 'none'"
            : "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; object-src 'none'";
    }
}
