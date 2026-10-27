using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Repositories;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Enrollment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .Include(e => e.Participant)
            .Include(e => e.CancelledBy)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByWorkshopIdAsync(int workshopId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Participant)
            .Include(e => e.CancelledBy)
            .Include(e => e.Workshop)
            .Where(e => e.WorkshopId == workshopId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByParticipantIdAsync(string participantId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
                .ThenInclude(w => w!.Instructor)
            .Include(e => e.Participant)
            .Include(e => e.CancelledBy)
            .Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetActiveEnrollmentAsync(string participantId, int workshopId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .FirstOrDefaultAsync(e => e.ParticipantId == participantId &&
                                      e.WorkshopId == workshopId &&
                                      e.Status == EnrollmentStatus.Active, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsForParticipantAsync(string participantId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Workshop)
            .Where(e => e.ParticipantId == participantId && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<(Workshop Workshop, int ActiveEnrollmentsCount)>> GetTopWorkshopsByActiveEnrollmentsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        var workshops = await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .ToListAsync(cancellationToken);

        return workshops
            .Select(w => (Workshop: w, ActiveEnrollmentsCount: w.GetActiveEnrollmentsCount()))
            .OrderByDescending(x => x.ActiveEnrollmentsCount)
            .ThenBy(x => x.Workshop.Title)
            .Take(limit)
            .ToList();
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
    }

    public void Update(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
    }
}
