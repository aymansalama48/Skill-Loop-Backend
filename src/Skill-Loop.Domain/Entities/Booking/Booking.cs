using Skill_Loop.Domain.Common.Entities;
using Skill_Loop.Domain.Enums;

namespace Skill_Loop.Domain.Entities.Booking;

public class Booking : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid LearnerUserId { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
}
