using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Repositories;

public interface IWorkshopRepository
{
    Task<Workshop?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByIdWithEnrollmentsAsync(int id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeWithEnrollmentsAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workshop>> GetFutureWorkshopsAsync(DateTime currentUtc, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workshop>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
    void Update(Workshop workshop);
}
