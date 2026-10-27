using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Repositories;

namespace SkillHub.Application.Services;

public class ReportService : IReportService
{
    private readonly IEnrollmentRepository _enrollmentRepository;

    public ReportService(IEnrollmentRepository enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var topWorkshops = await _enrollmentRepository.GetTopWorkshopsByActiveEnrollmentsAsync(limit, cancellationToken);

        return topWorkshops.Select(item => new TopWorkshopReportDto(
            item.Workshop.Id,
            item.Workshop.Code,
            item.Workshop.Title,
            item.Workshop.Instructor?.FullName ?? "Unknown",
            item.ActiveEnrollmentsCount,
            item.Workshop.Capacity,
            Math.Max(0, item.Workshop.Capacity - item.ActiveEnrollmentsCount)
        ));
    }
}
