using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Repositories;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByWorkshopIdAsync(int workshopId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByParticipantIdAsync(string participantId, CancellationToken cancellationToken = default);
    Task<Enrollment?> GetActiveEnrollmentAsync(string participantId, int workshopId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetActiveEnrollmentsForParticipantAsync(string participantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<(Workshop Workshop, int ActiveEnrollmentsCount)>> GetTopWorkshopsByActiveEnrollmentsAsync(int limit = 10, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    void Update(Enrollment enrollment);
}
