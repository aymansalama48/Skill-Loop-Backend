namespace Skill_Loop.UnitTests.Api.Middlewares;

using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Moq;
using Skill_Loop.Api.Middlewares;

/// <summary>
/// Tests for SecurityHeadersMiddleware.
///
/// The regression this guards against: the middleware shipped a
/// <c>default-src 'self'</c> CSP that applied to every response, including Scalar's
/// reference UI at <c>/scalar</c>. Scalar bootstraps from an inline
/// <c>&lt;script type="module"&gt;</c> block, which that policy blocks, so the docs page
/// returned HTTP 200 with a blank body and looked like a server fault rather than a
/// header problem.
/// </summary>
public class SecurityHeadersMiddlewareTests
{
    private const string DevelopmentName = "Development";
    private const string ProductionName = "Production";

    private static async Task<IHeaderDictionary> InvokeAsync(
        string path,
        string environmentName = DevelopmentName)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(e => e.EnvironmentName).Returns(environmentName);
        // IsDevelopment() is an extension method on IHostEnvironment, not a property on
        // IWebHostEnvironment, so Moq cannot stub it directly. Supplying EnvironmentName is
        // what the real implementation reads, and the middleware only ever calls the
        // extension, so this is the faithful setup.
        environment.SetupGet(e => e.EnvironmentName).Returns(environmentName);

        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask, environment.Object);

        await middleware.InvokeAsync(context);

        return context.Response.Headers;
    }

    [Theory]
    [InlineData("/api/v1/Courses")]
    [InlineData("/openapi/v1.json")]
    [InlineData("/scalar/v1")]
    public async Task BaselineHeaders_AreSetOnEveryResponse(string path)
    {
        var headers = await InvokeAsync(path);

        headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        headers["X-Frame-Options"].ToString().Should().Be("DENY");
        headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        headers["X-Permitted-Cross-Domain-Policies"].ToString().Should().Be("none");
        headers.ContainsKey("Content-Security-Policy").Should().BeTrue();
    }

    [Fact]
    public async Task ServerStackHeader_IsRemoved()
    {
        var headers = await InvokeAsync("/api/v1/Courses");

        headers.ContainsKey("X-Powered-By").Should().BeFalse();
    }

    [Theory]
    [InlineData("/scalar/v1")]
    [InlineData("/scalar")]
    public async Task ScalarUi_AllowsTheInlineBootstrapScript(string path)
    {
        var headers = await InvokeAsync(path);

        var csp = headers["Content-Security-Policy"].ToString();

        // Scalar's inline <script type="module"> cannot boot without this.
        csp.Should().Contain("script-src").And.Contain("'unsafe-inline'");
    }

    [Fact]
    public async Task ScalarUi_StillBlocksEvalAndFraming()
    {
        var headers = await InvokeAsync("/scalar/v1");

        var csp = headers["Content-Security-Policy"].ToString();

        csp.Should().Contain("frame-ancestors 'none'");
        csp.Should().Contain("object-src 'none'");
        // script-src-elem rather than a blanket script-src keeps eval disabled.
        csp.Should().NotContain("unsafe-eval");
    }

    [Fact]
    public async Task ApiResponses_KeepTheStrictPolicyWithNoInlineScripts()
    {
        var headers = await InvokeAsync("/api/v1/Courses");

        var csp = headers["Content-Security-Policy"].ToString();

        csp.Should().NotContain("unsafe-inline");
        csp.Should().Contain("default-src 'self'");
    }

    [Fact]
    public async Task ScalarUi_IsNotRelaxedOutsideDevelopment()
    {
        var headers = await InvokeAsync("/scalar/v1", ProductionName);

        // The docs UI is not even mapped in production, so the exemption must not leak there.
        headers["Content-Security-Policy"].ToString().Should().NotContain("unsafe-inline");
    }

    [Fact]
    public async Task PathsMerelyStartingWithScalar_AreNotExempted()
    {
        // Guards against StartsWithSegments matching more than intended on a crafted path.
        var headers = await InvokeAsync("/scalar-evil/admin");

        headers["Content-Security-Policy"].ToString().Should().NotContain("unsafe-inline");
    }
}
