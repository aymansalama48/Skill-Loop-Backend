using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.AddInstructorAvailability;
using Skill_Loop.Application.Features.Instructors.Commands.RemoveInstructorAvailability;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.RemoveInstructorAvailability;

public class RemoveInstructorAvailabilityCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly AddInstructorAvailabilityCommandHandler _addHandler;
    private readonly RemoveInstructorAvailabilityCommandHandler _handler;

    public RemoveInstructorAvailabilityCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _addHandler = new AddInstructorAvailabilityCommandHandler(_dbContext);
        _handler = new RemoveInstructorAvailabilityCommandHandler(_dbContext);
    }

    private async Task<InstructorProfile> SeedProfileWithAvailabilityAsync()
    {
        var userId = Guid.NewGuid();
        var profile = InstructorProfile.Create(userId, "Headline", "Bio").Data!;
        _dbContext.Add(profile);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        await _addHandler.Handle(
            new AddInstructorAvailabilityCommand(
                profile.Id, DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(12)),
            CancellationToken.None);

        return profile;
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundFailure()
    {
        var command = new RemoveInstructorAvailabilityCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenAvailabilityDoesNotExist_ReturnsNotFoundFailure()
    {
        var profile = await SeedProfileWithAvailabilityAsync();

        var command = new RemoveInstructorAvailabilityCommand(profile.Id, Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "InstructorProfile.AvailabilityNotFound");
    }

    [Fact]
    public async Task Handle_WithValidRequest_RemovesAvailabilityAndReturnsSuccess()
    {
        var profile = await SeedProfileWithAvailabilityAsync();

        var availability = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorAvailabilities.Where(a => a.InstructorProfileId == profile.Id),
            CancellationToken.None);

        var command = new RemoveInstructorAvailabilityCommand(profile.Id, availability!.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();

        var remaining = await _dbContext.CountAsync(
            _dbContext.InstructorAvailabilities.Where(a => a.InstructorProfileId == profile.Id),
            CancellationToken.None);
        remaining.Should().Be(0);
    }
}
