using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;

namespace SkillHub.Application.Interfaces;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopDto>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default);
    Task<WorkshopDetailDto> GetWorkshopByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WorkshopDetailDto> CreateWorkshopAsync(CreateWorkshopDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnrollmentDto>> GetWorkshopEnrollmentsAsync(int workshopId, CancellationToken cancellationToken = default);
}
