namespace SkillHub.Application.DTOs.Enrollments;

public record EnrollRequestDto(int WorkshopId);

public record EnrollmentDto(
    int Id,
    int WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    string ParticipantId,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancelledByName
);

public record ExcelImportResultDto(
    int TotalProcessed,
    int EnrolledCount,
    int RejectedCount,
    List<ExcelImportRowErrorDto> Errors
);

public record ExcelImportRowErrorDto(
    int RowNumber,
    string? ParticipantEmail,
    string? WorkshopCode,
    string Reason
);
