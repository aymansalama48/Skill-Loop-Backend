using FluentAssertions;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Abstractions.Persistence.Data;
using Skill_Loop.Application.Common.Errors.Identity;
using Skill_Loop.Application.Common.Errors.Instructors;
using Skill_Loop.Application.Features.Instructors.Commands.CreateMyInstructorProfile;
using Skill_Loop.Domain.Entities.Instructors;
using Skill_Loop.UnitTests.Common;

namespace Skill_Loop.UnitTests.Features.Instructors.Commands.CreateMyInstructorProfile;

public class CreateMyInstructorProfileCommandHandlerTests
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUser;
    private readonly CreateMyInstructorProfileCommandHandler _handler;

    public CreateMyInstructorProfileCommandHandlerTests()
    {
        _dbContext = InMemoryDbContextHelper.Create();
        _currentUser = new Mock<ICurrentUser>();
        _handler = new CreateMyInstructorProfileCommandHandler(_dbContext, _currentUser.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsNotFoundFailure()
    {
        _currentUser.Setup(c => c.UserId).Returns((Guid?)null);

        var command = new CreateMyInstructorProfileCommand(Guid.NewGuid(), "Headline", "Bio");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == UserErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserIsEmptyId_ReturnsNotFoundFailure()
    {
        _currentUser.Setup(c => c.UserId).Returns(Guid.Empty);

        var command = new CreateMyInstructorProfileCommand(Guid.Empty, "Headline", "Bio");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == UserErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenProfileAlreadyExists_ReturnsAlreadyExistsFailure()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);

        var existing = InstructorProfile.Create(userId, "Existing", "Bio").Data!;
        _dbContext.Add(existing);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var command = new CreateMyInstructorProfileCommand(userId, "Headline", "Bio");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == InstructorProfileErrors.AlreadyExists.Code);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesProfileAndReturnsId()
    {
        var userId = Guid.NewGuid();
        _currentUser.Setup(c => c.UserId).Returns(userId);

        var command = new CreateMyInstructorProfileCommand(userId, "Senior .NET Developer", "Experienced instructor.");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();

        var created = await _dbContext.FirstOrDefaultAsync(
            _dbContext.InstructorProfiles.Where(p => p.Id == result.Data));

        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);
        created.Headline.Should().Be("Senior .NET Developer");
        created.Bio.Should().Be("Experienced instructor.");
        created.IsApproved.Should().BeFalse();
    }
}
