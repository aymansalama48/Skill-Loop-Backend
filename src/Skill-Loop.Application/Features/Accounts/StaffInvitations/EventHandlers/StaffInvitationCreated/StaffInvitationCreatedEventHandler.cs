namespace Skill_Loop.Application.Features.Accounts.StaffInvitations.EventHandlers.StaffInvitationCreated;

using MediatR;
using Skill_Loop.Application.Common.Abstractions.Core;
using Skill_Loop.Application.Common.Abstractions.Events;
using Skill_Loop.Application.Common.Abstractions.External.Email.Models.Templates.IdentityTemplates;
using Skill_Loop.Application.Common.Abstractions.External.Jobs;
using Skill_Loop.Application.Common.Abstractions.Notifications;
using Skill_Loop.Domain.Constants;
using Skill_Loop.Domain.Entities.Invitation.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

public sealed class StaffInvitationCreatedEventHandler(IJobScheduler jobScheduler, IDateTime dateTime)
    : INotificationHandler<DomainEventNotification<StaffInvitationCreatedEvent>>
{
    public Task Handle(
        DomainEventNotification<StaffInvitationCreatedEvent> notification,
        CancellationToken cancellationToken)
    {
        var invitation = notification.DomainEvent.Invitation;  

        // حساب عدد الساعات المتبقية للصلاحية بناءً على الكيان الأصلي
        var expiryHours = (int)Math.Max(1, (invitation.ExpiresAtUtc - dateTime.Now).TotalHours);  

        // تجهيز الموديل بما يطابق جدولك تماماً
        var templateModel = new StaffInvitationTemplateModel
        {
            UserEmail = invitation.Email, // 👈 الخاصية الأساسية للقالب
            InvitedEmail = invitation.Email,  
            AdminName = invitation.AdminName,  
            Token = invitation.Token, // السيرفس ستقوم بتحويله لرابط كامل
            RoleName = TranslateRole(invitation.Role),  
            ExpiryHours = expiryHours 
        };

        // رمي المَهمة لـ Hangfire باستخدام الخدمة الموحدة للإشعارات
        jobScheduler.Enqueue<IIdentityNotificationService>(notificationService =>  
            notificationService.SendStaffInvitationEmailAsync(  
                invitation.Email,  
                templateModel));  

        return Task.CompletedTask; 
    }

    // دالة مساعدة لترجمة الـ Roles لتظهر في الإيميل بشكل جميل
    private static string TranslateRole(string role) => role switch
    {
        Roles.SuperAdmin => "المدير العام",
        Roles.Admin => "مدير النظام",
        Roles.FinanceManager => "المسؤول المالي",
        Roles.Support => "مسؤول الدعم",
        Roles.Instructor => "معلم",
        Roles.User => "مستخدم",
        _ => role
    };
}