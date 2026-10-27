using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(string participantUserId, int workshopId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(string participantUserId, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> CancelEnrollmentAsync(int enrollmentId, string currentUserId, bool isEmployee, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateCertificatePdfAsync(int enrollmentId, string currentUserId, bool isEmployee, CancellationToken cancellationToken = default);
    Task<ExcelImportResultDto> ImportFromExcelAsync(Stream excelStream, CancellationToken cancellationToken = default);
}
