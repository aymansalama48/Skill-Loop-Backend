namespace Skill_Loop.UnitTests.External.Cache;

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Xunit;

public class CacheServiceTests
{
    private readonly Mock<ICacheService> _cache;

    public CacheServiceTests()
    {
        _cache = new Mock<ICacheService>();
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotSet()
    {
        _cache.Setup(c => c.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _cache.Object.GetAsync<string>("missing-key", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsValue()
    {
        const string key = "test-key";
        const string value = "test-value";

        _cache.Setup(c => c.SetAsync(key, value, null, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _cache.Setup(c => c.GetAsync<string>(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);

        await _cache.Object.SetAsync(key, value, cancellationToken: CancellationToken.None);
        var result = await _cache.Object.GetAsync<string>(key, CancellationToken.None);

        result.Should().Be(value);
    }

    [Fact]
    public async Task RemoveAsync_CalledWithCorrectKey()
    {
        const string key = "remove-key";
        _cache.Setup(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cache.Object.RemoveAsync(key, CancellationToken.None);

        _cache.Verify(c => c.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_CalledWithCorrectPrefix()
    {
        const string prefix = "session:";
        _cache.Setup(c => c.RemoveByPrefixAsync(prefix, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _cache.Object.RemoveByPrefixAsync(prefix, CancellationToken.None);

        _cache.Verify(c => c.RemoveByPrefixAsync(prefix, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_ReturnsCachedValue_WhenExists()
    {
        const string key = "cached";
        const string cachedValue = "hello";

        _cache.Setup(c => c.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, Task<string>>>(),
                It.IsAny<Func<string, bool>>(),
                null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedValue);

        var result = await _cache.Object.GetOrCreateAsync(
            key,
            _ => Task.FromResult("world"),
            cancellationToken: CancellationToken.None);

        result.Should().Be(cachedValue);
    }

    [Fact]
    public async Task GetOrCreateAsync_CallsFactory_WhenNotCached()
    {
        const string key = "missing";
        const string newValue = "new";

        _cache.Setup(c => c.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, Task<string>>>(),
                It.IsAny<Func<string, bool>>(),
                null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newValue);

        var result = await _cache.Object.GetOrCreateAsync(
            key,
            _ => Task.FromResult(newValue),
            cancellationToken: CancellationToken.None);

        result.Should().Be(newValue);
    }
}
