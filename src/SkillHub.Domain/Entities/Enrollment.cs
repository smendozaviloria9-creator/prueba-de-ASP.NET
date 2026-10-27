using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }

    public int WorkshopId { get; set; }
    public virtual Workshop? Workshop { get; set; }

    public string ParticipantId { get; set; } = string.Empty;
    public virtual ApplicationUser? Participant { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public DateTime? CancelledAt { get; set; }
    public string? CancelledById { get; set; }
    public virtual ApplicationUser? CancelledBy { get; set; }

    public Enrollment()
    {
    }

    public Enrollment(int workshopId, string participantId, DateTime enrolledAt)
    {
        WorkshopId = workshopId;
        ParticipantId = participantId;
        EnrolledAt = enrolledAt;
        Status = EnrollmentStatus.Active;
    }

    public void Cancel(string cancelledById, bool isEmployee, DateTime currentUtc)
    {
        if (Status == EnrollmentStatus.Cancelled)
            throw new BusinessRuleException("The enrollment is already cancelled.");

        if (Workshop == null)
            throw new DomainException("Workshop navigation property must be loaded to validate cancellation rules.");

        if (Workshop.HasStarted(currentUtc))
            throw new BusinessRuleException("Cannot cancel an enrollment for a workshop that has already started.");

        if (!isEmployee)
        {
            TimeSpan timeUntilStart = Workshop.StartDateTime - currentUtc;
            if (timeUntilStart < TimeSpan.FromHours(24))
            {
                throw new BusinessRuleException("Participants can only cancel an enrollment at least 24 hours before the workshop starts.");
            }
        }

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = currentUtc;
        CancelledById = cancelledById;
    }
}
