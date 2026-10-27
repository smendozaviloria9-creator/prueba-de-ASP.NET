using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Domain.Entities;

public class Workshop
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string InstructorId { get; set; } = string.Empty;
    public virtual ApplicationUser? Instructor { get; set; }

    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int Capacity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public Workshop()
    {
    }

    public Workshop(string code, string title, string description, string instructorId, DateTime startDateTime, DateTime endDateTime, int capacity, DateTime currentUtc)
    {
        Code = code.Trim().ToUpperInvariant();
        Title = title.Trim();
        Description = description.Trim();
        InstructorId = instructorId;
        StartDateTime = startDateTime;
        EndDateTime = endDateTime;
        Capacity = capacity;
        CreatedAt = currentUtc;

        Validate(currentUtc);
    }

    public void Validate(DateTime currentUtc)
    {
        if (string.IsNullOrWhiteSpace(Code))
            throw new BusinessRuleException("The workshop code cannot be empty.");

        if (string.IsNullOrWhiteSpace(Title))
            throw new BusinessRuleException("The workshop title cannot be empty.");

        if (string.IsNullOrWhiteSpace(InstructorId))
            throw new BusinessRuleException("The workshop must have an assigned instructor.");

        if (StartDateTime <= currentUtc)
            throw new BusinessRuleException("A workshop must be scheduled with a future start date and time.");

        if (EndDateTime <= StartDateTime)
            throw new BusinessRuleException("The workshop end date and time must be after the start date and time.");

        if (Capacity <= 0)
            throw new BusinessRuleException("Workshop maximum capacity must be greater than zero.");
    }

    public int GetActiveEnrollmentsCount()
    {
        return Enrollments?.Count(e => e.Status == EnrollmentStatus.Active) ?? 0;
    }

    public int GetAvailableCapacity()
    {
        return Math.Max(0, Capacity - GetActiveEnrollmentsCount());
    }

    public bool HasStarted(DateTime currentUtc)
    {
        return currentUtc >= StartDateTime;
    }

    public bool OverlapsWith(DateTime start, DateTime end)
    {
        return StartDateTime < end && EndDateTime > start;
    }
}
