using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.AddInstructorAvailability;

public class AddInstructorAvailabilityCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AddInstructorAvailabilityCommandHandler _handler;

    public AddInstructorAvailabilityCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _handler = new AddInstructorAvailabilityCommandHandler(_dbContext);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new AddInstructorAvailabilityCommand(
            Guid.NewGuid(), DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(10));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WithValidRequest_AddsAvailabilityAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new AddInstructorAvailabilityCommand(
            profile.Id, DayOfWeek.Tuesday, TimeSpan.FromHours(14), TimeSpan.FromHours(16));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);

        var persisted = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorAvailabilities.Where(a => a.InstructorProfileId == profile.Id),
            CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.DayOfWeek.Should().Be(DayOfWeek.Tuesday);
        persisted.Id.Should().Be(result.Data);
    }

    [Fact]
    public async Task Handle_WhenTimeOverlapsExisting_ReturnsConflictFailure()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var existing = new AddInstructorAvailabilityCommand(
            profile.Id, DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(12));
        await _handler.Handle(existing, CancellationToken.None);

        var overlapping = new AddInstructorAvailabilityCommand(
            profile.Id, DayOfWeek.Monday, TimeSpan.FromHours(10), TimeSpan.FromHours(14));

        var result = await _handler.Handle(overlapping, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorProfile.AvailabilityOverlap");
    }
}
