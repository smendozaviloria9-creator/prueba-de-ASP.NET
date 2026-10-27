namespace SkillHub.Application.DTOs.Reports;

public record TopWorkshopReportDto(
    int WorkshopId,
    string Code,
    string Title,
    string InstructorName,
    int ActiveEnrollmentsCount,
    int Capacity,
    int AvailableCapacity
);
