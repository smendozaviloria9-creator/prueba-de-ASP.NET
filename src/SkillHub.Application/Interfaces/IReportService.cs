using SkillHub.Application.DTOs.Reports;

namespace SkillHub.Application.Interfaces;

public interface IReportService
{
    Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsAsync(int limit = 10, CancellationToken cancellationToken = default);
}
