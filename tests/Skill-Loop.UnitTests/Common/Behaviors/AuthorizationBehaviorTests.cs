using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using Skill_Loop.Application.Common.Abstractions.Identity.Authorization;
using Skill_Loop.Application.Common.Abstractions.Identity.CurrentUser;
using Skill_Loop.Application.Common.Behaviors;
using Skill_Loop.Application.Common.Errors.Auth;
using Skill_Loop.Application.Common.Abstractions.Messaging;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Constants;
using Xunit;

namespace Skill_Loop.UnitTests.Common.Behaviors;

public class AuthorizationBehaviorTests
{
    private static Mock<ICurrentUser> BuildCurrentUser(params string[] permissions)
    {
        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(c => c.IsAuthenticated).Returns(true);
        currentUser.Setup(c => c.UserId).Returns(Guid.NewGuid());
        currentUser.Setup(c => c.HasPermission(It.IsAny<string>()))
            .Returns<string>(p => Array.IndexOf(permissions, p) >= 0);
        return currentUser;
    }

    [Permission(Permissions.Courses.Create)]
    private sealed record CourseCreateRequest : IRequest<Result>;

    [Permission(Permissions.Finance.PromoCodesManage)]
    private sealed record PromoCodeRequest : IRequest<Result>;

    private static readonly RequestHandlerDelegate<Result> NextHandler =
        () => Task.FromResult(Result.Success());

    [Fact]
    public async Task Handle_WithExactPermission_CallsNext()
    {
        var behavior = new AuthorizationBehavior<CourseCreateRequest, Result>(
            BuildCurrentUser(Permissions.Courses.Create).Object);

        var result = await behavior.Handle(new CourseCreateRequest(), NextHandler, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithManageAllInsteadOfGranularPermission_CallsNext()
    {
        // Admin holds Courses.ManageAll but not Courses.Create.
        var behavior = new AuthorizationBehavior<CourseCreateRequest, Result>(
            BuildCurrentUser(Permissions.Courses.ManageAll).Object);

        var result = await behavior.Handle(new CourseCreateRequest(), NextHandler, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithUnrelatedPermission_ReturnsForbidden()
    {
        var behavior = new AuthorizationBehavior<CourseCreateRequest, Result>(
            BuildCurrentUser(Permissions.Sessions.Create).Object);

        var result = await behavior.Handle(new CourseCreateRequest(), NextHandler, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(AuthErrors.Forbidden);
    }

    [Fact]
    public async Task Handle_WithNoPermissions_ReturnsForbidden()
    {
        var behavior = new AuthorizationBehavior<CourseCreateRequest, Result>(
            BuildCurrentUser().Object);

        var result = await behavior.Handle(new CourseCreateRequest(), NextHandler, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(AuthErrors.Forbidden);
    }

    [Fact]
    public async Task Handle_WithFinanceManageAll_SatisfiesPromoCodesManage()
    {
        var behavior = new AuthorizationBehavior<PromoCodeRequest, Result>(
            BuildCurrentUser(Permissions.Finance.ManageAll).Object);

        var result = await behavior.Handle(new PromoCodeRequest(), NextHandler, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}