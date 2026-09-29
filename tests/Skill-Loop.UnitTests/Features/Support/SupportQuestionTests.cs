using FluentAssertions;
using Skill_Loop.Domain.Common.Results;
using Skill_Loop.Domain.Entities.Support;

namespace Skill_Loop.UnitTests.Features.Support;

public class SupportQuestionTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesUnansweredUnpublishedQuestion()
    {
        var result = SupportQuestion.Create("How do I reset my password?", "Account");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Question.Should().Be("How do I reset my password?");
        result.Data.Category.Should().Be("Account");
        result.Data.IsPublished.Should().BeFalse();
        result.Data.IsAnswered.Should().BeFalse();
        result.Data.Answer.Should().BeNull();
        result.Data.EmailSent.Should().BeFalse();
    }

    [Fact]
    public void Create_WithUserDetails_StoresAskerInformation()
    {
        var userId = Guid.NewGuid();

        var result = SupportQuestion.Create("Q", "General", "user@test.com", "User", userId);

        result.Data!.UserEmail.Should().Be("user@test.com");
        result.Data.UserName.Should().Be("User");
        result.Data.AskedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Create_WithoutUserId_LeavesAskedByUserIdNull()
    {
        var result = SupportQuestion.Create("Q", "General");

        result.Data!.AskedByUserId.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyQuestion_ReturnsValidationError(string question)
    {
        var result = SupportQuestion.Create(question, "General");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyQuestion" && e.Type == ErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyCategory_ReturnsValidationError(string category)
    {
        var result = SupportQuestion.Create("Q", category);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyCategory");
    }

    [Fact]
    public void Create_TrimsValues()
    {
        var result = SupportQuestion.Create("  Q  ", "  General  ", "  a@b.com  ");

        result.Data!.Question.Should().Be("Q");
        result.Data.Category.Should().Be("General");
        result.Data.UserEmail.Should().Be("a@b.com");
    }

    [Fact]
    public void CreatePublished_WithAnswer_MarksAnsweredAndPublished()
    {
        var result = SupportQuestion.CreatePublished("Q", "A", "General");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Answer.Should().Be("A");
        result.Data.IsPublished.Should().BeTrue();
        result.Data.IsAnswered.Should().BeTrue();
        result.Data.AnsweredAt.Should().NotBeNull();
    }

    [Fact]
    public void CreatePublished_WithEmptyAnswer_ReturnsValidationError()
    {
        var result = SupportQuestion.CreatePublished("Q", "  ", "General");

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyAnswer");
    }

    [Fact]
    public void SetAnswer_SetsAnswerAndMarksAnswered()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;

        var result = question.SetAnswer("  The answer  ");

        result.IsSuccess.Should().BeTrue();
        question.Answer.Should().Be("The answer");
        question.IsAnswered.Should().BeTrue();
        question.AnsweredAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void SetAnswer_WithEmptyAnswer_ReturnsValidationErrorAndLeavesStateUnchanged(string answer)
    {
        var question = SupportQuestion.Create("Q", "General").Data!;

        var result = question.SetAnswer(answer);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyAnswer");
        question.IsAnswered.Should().BeFalse();
        question.Answer.Should().BeNull();
    }

    [Fact]
    public void Publish_WithoutAnswer_Fails()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;

        var result = question.Publish();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.NoAnswer" && e.Type == ErrorType.Validation);
        question.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Publish_WithAnswer_Succeeds()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;
        question.SetAnswer("A");

        var result = question.Publish();

        result.IsSuccess.Should().BeTrue();
        question.IsPublished.Should().BeTrue();
    }

    [Fact]
    public void Unpublish_TurnsOffPublishedFlag()
    {
        var question = SupportQuestion.CreatePublished("Q", "A", "General").Data!;

        question.Unpublish();

        question.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void MarkEmailSent_StampsFlagAndTimestamp()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;

        var result = question.MarkEmailSent();

        result.IsSuccess.Should().BeTrue();
        question.EmailSent.Should().BeTrue();
        question.EmailSentAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithAnswer_FlipsIsAnsweredOnFirstAnswerOnly()
    {
        var question = SupportQuestion.Create("Q", "General").Data!;

        question.Update("New Q", "New A", "Billing", true);

        question.Question.Should().Be("New Q");
        question.Answer.Should().Be("New A");
        question.Category.Should().Be("Billing");
        question.IsPublished.Should().BeTrue();
        question.IsAnswered.Should().BeTrue();
        var firstAnsweredAt = question.AnsweredAt;

        question.Update("Newer Q", "Newer A", "Billing", true);

        question.AnsweredAt.Should().Be(firstAnsweredAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Update_WithEmptyQuestion_ReturnsValidationError(string question)
    {
        var entity = SupportQuestion.Create("Q", "General").Data!;

        var result = entity.Update(question, "A", "General", true);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == "SupportQuestion.EmptyQuestion");
    }

    [Fact]
    public void Create_GeneratesDistinctIds()
    {
        var first = SupportQuestion.Create("Q1", "General").Data!;
        var second = SupportQuestion.Create("Q2", "General").Data!;

        first.Id.Should().NotBe(second.Id);
    }
}
