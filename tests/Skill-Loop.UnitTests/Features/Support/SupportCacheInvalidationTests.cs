using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.External.Cache;
using Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.CreateSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.DeleteSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.SetSupportQuestionPublication;
using Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;
using Skill_Loop.Application.Features.Support.Commands.UpdateSupportQuestion;
using Skill_Loop.Application.Features.Support.Share;

namespace Skill_Loop.UnitTests.Features.Support;

public class SupportCacheInvalidationTests
{
    public static TheoryData<ICacheInvalidatorCommand> AllCommands() => new()
    {
        new SubmitContactFormCommand("Subject", "Message"),
        new CreateSupportQuestionCommand("Q", "A", "General", true),
        new AnswerSupportQuestionCommand(Guid.NewGuid(), "A"),
        new UpdateSupportQuestionCommand(Guid.NewGuid(), "Q", "A", "General", true),
        new SetSupportQuestionPublicationCommand(Guid.NewGuid(), true),
        new DeleteSupportQuestionCommand(Guid.NewGuid()),
    };

    [Theory]
    [MemberData(nameof(AllCommands))]
    public void CacheKeys_CoverEverySupportQueryNamespace(ICacheInvalidatorCommand command)
    {
        // المسح بالبادئة، فأي بادئة ناقصة معناها كاش قديم بيفضل يخدع المستخدمين.
        command.CacheKeys.Should().Contain(new[] { SupportCacheKeys.Questions });
        command.CacheKeys.Should().Contain(new[] { SupportCacheKeys.QuestionById });
        command.CacheKeys.Should().Contain(new[] { SupportCacheKeys.MyQuestions });
    }
}
