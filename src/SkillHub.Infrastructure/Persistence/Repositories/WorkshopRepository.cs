using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Repositories;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public class WorkshopRepository : IWorkshopRepository
{
    private readonly ApplicationDbContext _context;

    public WorkshopRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Workshop?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Workshops
            .Include(w => w.Instructor)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task<Workshop?> GetByIdWithEnrollmentsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workshop?> GetByCodeWithEnrollmentsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .FirstOrDefaultAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task<IEnumerable<Workshop>> GetFutureWorkshopsAsync(DateTime currentUtc, CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .Where(w => w.StartDateTime > currentUtc)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workshop>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workshops
            .Include(w => w.Instructor)
            .Include(w => w.Enrollments)
            .OrderBy(w => w.StartDateTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.Workshops.AnyAsync(w => w.Code == normalizedCode, cancellationToken);
    }

    public async Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default)
    {
        await _context.Workshops.AddAsync(workshop, cancellationToken);
    }

    public void Update(Workshop workshop)
    {
        _context.Workshops.Update(workshop);
    }
}
