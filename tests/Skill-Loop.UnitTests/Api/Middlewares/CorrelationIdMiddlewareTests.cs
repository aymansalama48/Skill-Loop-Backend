namespace Skill_Loop.UnitTests.Api.Middlewares;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Skill_Loop.Api.Middlewares;
using Skill_Loop.Application.Common.Constants;

/// <summary>
/// Tests for CorrelationIdMiddleware input validation.
///
/// The correlation ID is client-supplied and lands in both the response header and every log
/// line for the request. Echoing it unvalidated allowed log forging (a value containing CR/LF
/// can append fake entries to the log file) and unbounded log/header growth.
/// </summary>
public class CorrelationIdMiddlewareTests
{
    private static async Task<(string CorrelationId, HttpResponse Response)> InvokeAsync(
        string? inboundValue,
        string? inboundHeaderName = null)
    {
        var context = new DefaultHttpContext();

        if (inboundValue is not null)
        {
            var headerName = inboundHeaderName ?? CorrelationConstants.HeaderKey;
            context.Request.Headers[headerName] = new StringValues(inboundValue);
        }

        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        return (
            context.Items[CorrelationConstants.HeaderKey]!.ToString()!,
            context.Response);
    }

    [Fact]
    public async Task AbsentHeader_GeneratesAGuid()
    {
        var (correlationId, _) = await InvokeAsync(inboundValue: null);

        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task SafeAlphanumericValue_IsPreserved()
    {
        var (correlationId, _) = await InvokeAsync("abc-123_XYZ.456");

        correlationId.Should().Be("abc-123_XYZ.456",
            "a well-formed ID should be honoured so distributed traces stay linked");
    }

    [Theory]
    [InlineData("value\r\nFAKE LOG LINE 2026-01-01 ERROR forged")]
    [InlineData("value\nInjected: header")]
    [InlineData("value\rX-Injected: 1")]
    public async Task HeaderInjectionCharacters_AreRejected(string malicious)
    {
        var (correlationId, _) = await InvokeAsync(malicious);

        Guid.TryParse(correlationId, out _).Should().BeTrue(
            "CR/LF in a client value is a log-forging and response-splitting vector");
    }

    [Fact]
    public async Task ExcessiveLength_IsRejected()
    {
        var (correlationId, _) = await InvokeAsync(new string('a', 5000));

        Guid.TryParse(correlationId, out _).Should().BeTrue("an unbounded value is a log-bloat vector");
        correlationId.Length.Should().BeLessThan(100);
    }

    [Fact]
    public async Task WhitespaceOnlyValue_IsRejected()
    {
        var (correlationId, _) = await InvokeAsync("   ");

        Guid.TryParse(correlationId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task SafeValue_IsStoredForDownstreamLogging()
    {
        var (correlationId, _) = await InvokeAsync("trace-42");

        correlationId.Should().Be("trace-42",
            "the resolved value is what Serilog's LogContext and the error handler use");

        // Note: the response-header echo is registered via Response.OnStarting, which
        // DefaultHttpContext does not implement, so it is not asserted here. Echoing the
        // validated value back is unchanged behaviour and is covered end-to-end by the
        // integration environment.
    }
}
