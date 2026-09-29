namespace Skill_Loop.UnitTests.External.Notifications;

using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Application.Features.Support.Commands.AnswerSupportQuestion;
using Skill_Loop.Application.Features.Support.Commands.SendFaqAnswerEmail;
using Skill_Loop.Application.Features.Support.Commands.SubmitContactForm;
using Skill_Loop.Infrastructure.Notifications;
using Xunit;

/// <summary>
/// ثوابت تصميمية (ISP): واجهات إشعارات الدعم مقسومة بحسب الاستهلاك،
/// و<b>مش</b> في واجهة واحدة كبيرة بتجبر كل عملية تعتمد على methods مش محتاجاها.
/// </summary>
public class SupportNotificationContractsTests
{
    [Fact]
    public void NotifierInterfaces_AreSeparatedNotCombined()
    {
        typeof(ISupportRequestNotifier).IsAssignableFrom(typeof(ISupportAnswerNotifier)).Should().BeFalse();
        typeof(ISupportAnswerNotifier).IsAssignableFrom(typeof(ISupportRequestNotifier)).Should().BeFalse();
    }

    [Fact]
    public void NotifierInterfaces_ExposeOnlyTheirOwnConcerns()
    {
        typeof(ISupportRequestNotifier).GetMethods().Select(m => m.Name)
            .Should().BeEquivalentTo(["SendContactFormConfirmationAsync", "SendSupportTeamNotificationAsync"]);

        typeof(ISupportAnswerNotifier).GetMethods().Select(m => m.Name)
            .Should().BeEquivalentTo(["SendAnswerNotificationAsync", "SendFaqAnswerAsync"]);
    }

    [Fact]
    public void SupportNotificationService_ImplementsBothNarrowInterfaces()
    {
        var service = typeof(SupportNotificationService);

        service.Should().BeAssignableTo<ISupportRequestNotifier>();
        service.Should().BeAssignableTo<ISupportAnswerNotifier>();
    }

    [Theory]
    [InlineData(typeof(AnswerSupportQuestionCommandHandler))]
    [InlineData(typeof(SendFaqAnswerEmailCommandHandler))]
    [InlineData(typeof(SubmitContactFormCommandHandler))]
    public void Handlers_DependOnAbstractions_NotConcreteNotificationService(Type handlerType)
    {
        // DIP: الـ Handlers مينفعش Depends on concrete type.
        var fieldTypes = handlerType
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Select(f => f.FieldType);

        fieldTypes.Should().NotContain(typeof(SupportNotificationService));
        fieldTypes.Should().OnlyContain(t => t.IsInterface, "handlers should depend on abstractions");
    }
}
