using Skill_Loop.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skill_Loop.Domain.Entities.Invitation.Events
{
    public sealed record StaffInvitationCreatedEvent(StaffInvitation Invitation) : IDomainEvent;

}
