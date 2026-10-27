namespace SkillHub.Application.DTOs.Workshops;

public record WorkshopDto(
    int Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity,
    int AvailableCapacity,
    int ActiveEnrollmentsCount
);

public record WorkshopDetailDto(
    int Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    string? InstructorSpecialty,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity,
    int AvailableCapacity,
    int ActiveEnrollmentsCount
);

public record CreateWorkshopDto(
    string Code,
    string Title,
    string Description,
    string InstructorEmail,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int Capacity
);
