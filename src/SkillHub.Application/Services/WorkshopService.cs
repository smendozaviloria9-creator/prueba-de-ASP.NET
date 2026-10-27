using Microsoft.AspNetCore.Identity;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;

namespace SkillHub.Application.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public WorkshopService(
        IWorkshopRepository workshopRepository,
        IEnrollmentRepository enrollmentRepository,
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _workshopRepository = workshopRepository;
        _enrollmentRepository = enrollmentRepository;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WorkshopDto>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var workshops = await _workshopRepository.GetFutureWorkshopsAsync(now, cancellationToken);

        return workshops.Select(w => new WorkshopDto(
            w.Id,
            w.Code,
            w.Title,
            w.Description,
            w.Instructor?.FullName ?? "Unknown",
            w.Instructor?.Email ?? string.Empty,
            w.StartDateTime,
            w.EndDateTime,
            w.Capacity,
            w.GetAvailableCapacity(),
            w.GetActiveEnrollmentsCount()
        ));
    }

    public async Task<WorkshopDetailDto> GetWorkshopByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdWithEnrollmentsAsync(id, cancellationToken);
        if (workshop == null)
            throw new NotFoundException($"Workshop with ID {id} was not found.");

        return new WorkshopDetailDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            workshop.Instructor?.FullName ?? "Unknown",
            workshop.Instructor?.Email ?? string.Empty,
            workshop.Instructor?.Specialty,
            workshop.StartDateTime,
            workshop.EndDateTime,
            workshop.Capacity,
            workshop.GetAvailableCapacity(),
            workshop.GetActiveEnrollmentsCount()
        );
    }

    public async Task<WorkshopDetailDto> CreateWorkshopAsync(CreateWorkshopDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new BusinessRuleException("Workshop unique code is required.");

        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (await _workshopRepository.ExistsCodeAsync(normalizedCode, cancellationToken))
            throw new BusinessRuleException($"A workshop with code '{normalizedCode}' already exists.");

        if (string.IsNullOrWhiteSpace(request.InstructorEmail))
            throw new BusinessRuleException("Instructor email is required.");

        var instructor = await _userManager.FindByEmailAsync(request.InstructorEmail.Trim());
        if (instructor == null || instructor.Role != UserRoles.Employee)
            throw new BusinessRuleException($"Instructor with email '{request.InstructorEmail}' does not exist or is not a registered employee/instructor.");

        var now = DateTime.UtcNow;

        var workshop = new Workshop(
            normalizedCode,
            request.Title,
            request.Description,
            instructor.Id,
            request.StartDateTime,
            request.EndDateTime,
            request.Capacity,
            now
        );

        await _workshopRepository.AddAsync(workshop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkshopDetailDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            instructor.FullName,
            instructor.Email!,
            instructor.Specialty,
            workshop.StartDateTime,
            workshop.EndDateTime,
            workshop.Capacity,
            workshop.Capacity,
            0
        );
    }

    public async Task<IEnumerable<EnrollmentDto>> GetWorkshopEnrollmentsAsync(int workshopId, CancellationToken cancellationToken = default)
    {
        var workshop = await _workshopRepository.GetByIdAsync(workshopId, cancellationToken);
        if (workshop == null)
            throw new NotFoundException($"Workshop with ID {workshopId} was not found.");

        var enrollments = await _enrollmentRepository.GetByWorkshopIdAsync(workshopId, cancellationToken);

        return enrollments.Select(e => new EnrollmentDto(
            e.Id,
            e.WorkshopId,
            e.Workshop?.Code ?? string.Empty,
            e.Workshop?.Title ?? string.Empty,
            e.ParticipantId,
            e.Participant?.FullName ?? "Unknown",
            e.Participant?.Email ?? string.Empty,
            e.Status.ToString(),
            e.EnrolledAt,
            e.CancelledAt,
            e.CancelledBy?.FullName
        ));
    }
}
