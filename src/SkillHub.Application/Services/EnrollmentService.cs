using Microsoft.AspNetCore.Identity;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Repositories;

namespace SkillHub.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWorkshopRepository _workshopRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPdfReportGenerator _pdfReportGenerator;
    private readonly IExcelReader _excelReader;
    private readonly IUnitOfWork _unitOfWork;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IWorkshopRepository workshopRepository,
        UserManager<ApplicationUser> userManager,
        IPdfReportGenerator pdfReportGenerator,
        IExcelReader excelReader,
        IUnitOfWork unitOfWork)
    {
        _enrollmentRepository = enrollmentRepository;
        _workshopRepository = workshopRepository;
        _userManager = userManager;
        _pdfReportGenerator = pdfReportGenerator;
        _excelReader = excelReader;
        _unitOfWork = unitOfWork;
    }

    public async Task<EnrollmentDto> EnrollAsync(string participantUserId, int workshopId, CancellationToken cancellationToken = default)
    {
        var participant = await _userManager.FindByIdAsync(participantUserId);
        if (participant == null)
            throw new NotFoundException("Participant not found.");

        var workshop = await _workshopRepository.GetByIdWithEnrollmentsAsync(workshopId, cancellationToken);
        if (workshop == null)
            throw new NotFoundException($"Workshop with ID {workshopId} not found.");

        var now = DateTime.UtcNow;

        // Rule 4: Only future workshops
        if (workshop.HasStarted(now))
            throw new BusinessRuleException("Cannot enroll in a workshop that has already started.");

        // Rule 1: Capacity
        if (workshop.GetAvailableCapacity() <= 0)
            throw new BusinessRuleException("Cannot enroll: Workshop maximum capacity has been reached.");

        // Rule 2: No duplicates (active)
        var existingActive = await _enrollmentRepository.GetActiveEnrollmentAsync(participantUserId, workshopId, cancellationToken);
        if (existingActive != null)
            throw new BusinessRuleException("Participant already has an active enrollment in this workshop.");

        // Rule 3: No schedule conflicts
        var activeEnrollments = await _enrollmentRepository.GetActiveEnrollmentsForParticipantAsync(participantUserId, cancellationToken);
        var conflictingEnrollment = activeEnrollments.FirstOrDefault(e =>
            e.Workshop != null && e.Workshop.OverlapsWith(workshop.StartDateTime, workshop.EndDateTime));

        if (conflictingEnrollment != null)
        {
            throw new BusinessRuleException(
                $"Schedule conflict: Participant is already enrolled in '{conflictingEnrollment.Workshop?.Title}' ({conflictingEnrollment.Workshop?.Code}) from {conflictingEnrollment.Workshop?.StartDateTime:yyyy-MM-dd HH:mm} to {conflictingEnrollment.Workshop?.EndDateTime:yyyy-MM-dd HH:mm}.");
        }

        var enrollment = new Enrollment(workshop.Id, participant.Id, now);
        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EnrollmentDto(
            enrollment.Id,
            workshop.Id,
            workshop.Code,
            workshop.Title,
            participant.Id,
            participant.FullName,
            participant.Email!,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt,
            enrollment.CancelledAt,
            null
        );
    }

    public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(string participantUserId, CancellationToken cancellationToken = default)
    {
        var enrollments = await _enrollmentRepository.GetByParticipantIdAsync(participantUserId, cancellationToken);

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

    public async Task<EnrollmentDto> CancelEnrollmentAsync(int enrollmentId, string currentUserId, bool isEmployee, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId, cancellationToken);
        if (enrollment == null)
            throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");

        if (!isEmployee && enrollment.ParticipantId != currentUserId)
        {
            throw new BusinessRuleException("Participants can only view and manage their own enrollments.");
        }

        var now = DateTime.UtcNow;
        enrollment.Cancel(currentUserId, isEmployee, now);

        _enrollmentRepository.Update(enrollment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var canceller = await _userManager.FindByIdAsync(currentUserId);

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.WorkshopId,
            enrollment.Workshop?.Code ?? string.Empty,
            enrollment.Workshop?.Title ?? string.Empty,
            enrollment.ParticipantId,
            enrollment.Participant?.FullName ?? "Unknown",
            enrollment.Participant?.Email ?? string.Empty,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt,
            enrollment.CancelledAt,
            canceller?.FullName ?? "System"
        );
    }

    public async Task<byte[]> GenerateCertificatePdfAsync(int enrollmentId, string currentUserId, bool isEmployee, CancellationToken cancellationToken = default)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithDetailsAsync(enrollmentId, cancellationToken);
        if (enrollment == null)
            throw new NotFoundException($"Enrollment with ID {enrollmentId} not found.");

        if (!isEmployee && enrollment.ParticipantId != currentUserId)
        {
            throw new BusinessRuleException("Participants can only download certificates for their own enrollments.");
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            throw new BusinessRuleException("Enrollment certificates are only available for active enrollments.");
        }

        var data = new EnrollmentCertificateData(
            OrganizationName: "SkillHub",
            CertificateTitle: "Constancia de inscripción",
            ParticipantName: enrollment.Participant?.FullName ?? "Participant",
            ParticipantEmail: enrollment.Participant?.Email ?? string.Empty,
            WorkshopCode: enrollment.Workshop?.Code ?? string.Empty,
            WorkshopTitle: enrollment.Workshop?.Title ?? string.Empty,
            InstructorName: enrollment.Workshop?.Instructor?.FullName ?? "Instructor",
            WorkshopStartDateTime: enrollment.Workshop?.StartDateTime ?? DateTime.MinValue,
            WorkshopEndDateTime: enrollment.Workshop?.EndDateTime ?? DateTime.MinValue,
            EnrolledAt: enrollment.EnrolledAt,
            GeneratedAt: DateTime.UtcNow
        );

        return _pdfReportGenerator.GenerateEnrollmentCertificate(data);
    }

    public async Task<ExcelImportResultDto> ImportFromExcelAsync(Stream excelStream, CancellationToken cancellationToken = default)
    {
        var rows = _excelReader.ReadEnrollmentRows(excelStream).ToList();
        var errors = new List<ExcelImportRowErrorDto>();
        var validEnrollmentsToSave = new List<Enrollment>();
        var now = DateTime.UtcNow;

        // Cache users and workshops for efficient bulk checking
        var workshopsByCode = (await _workshopRepository.GetAllAsync(cancellationToken))
            .ToDictionary(w => w.Code.Trim().ToUpperInvariant(), w => w);

        // Keep in-memory tracking of active enrollments by participant and workshop capacities
        var workshopActiveCounts = workshopsByCode.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.GetActiveEnrollmentsCount());

        var participantActiveEnrollments = new Dictionary<string, List<(int WorkshopId, string WorkshopCode, DateTime Start, DateTime End)>>();

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.ParticipantEmail))
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, "Participant email is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.WorkshopCode))
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, "Workshop code is required."));
                continue;
            }

            var normalizedCode = row.WorkshopCode.Trim().ToUpperInvariant();
            var normalizedEmail = row.ParticipantEmail.Trim();

            // Check participant existence
            var participant = await _userManager.FindByEmailAsync(normalizedEmail);
            if (participant == null)
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, $"Participant with email '{row.ParticipantEmail}' does not exist."));
                continue;
            }

            // Check workshop existence
            if (!workshopsByCode.TryGetValue(normalizedCode, out var workshop))
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, $"Workshop with code '{row.WorkshopCode}' does not exist."));
                continue;
            }

            // Rule 4: Future workshop only
            if (workshop.HasStarted(now))
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, $"Workshop '{workshop.Code}' has already started."));
                continue;
            }

            // Rule 1: Capacity check
            var currentActive = workshopActiveCounts[normalizedCode];
            if (currentActive >= workshop.Capacity)
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, $"Workshop '{workshop.Code}' has reached its maximum capacity of {workshop.Capacity}."));
                continue;
            }

            // Load existing active enrollments into participant map if not already loaded
            if (!participantActiveEnrollments.ContainsKey(participant.Id))
            {
                var existingActive = await _enrollmentRepository.GetActiveEnrollmentsForParticipantAsync(participant.Id, cancellationToken);
                participantActiveEnrollments[participant.Id] = existingActive
                    .Where(e => e.Workshop != null)
                    .Select(e => (e.WorkshopId, e.Workshop!.Code, e.Workshop.StartDateTime, e.Workshop.EndDateTime))
                    .ToList();
            }

            var participantWorkshops = participantActiveEnrollments[participant.Id];

            // Rule 2: Duplicate check
            if (participantWorkshops.Any(w => w.WorkshopId == workshop.Id))
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode, $"Participant '{row.ParticipantEmail}' is already actively enrolled in workshop '{workshop.Code}'."));
                continue;
            }

            // Rule 3: Schedule conflict check
            var conflict = participantWorkshops.FirstOrDefault(w => workshop.OverlapsWith(w.Start, w.End));
            if (conflict != default)
            {
                errors.Add(new ExcelImportRowErrorDto(row.RowNumber, row.ParticipantEmail, row.WorkshopCode,
                    $"Schedule conflict: Participant has an active enrollment in workshop '{conflict.WorkshopCode}' ({conflict.Start:yyyy-MM-dd HH:mm} - {conflict.End:yyyy-MM-dd HH:mm}) overlapping with this workshop."));
                continue;
            }

            // Valid row!
            var enrollment = new Enrollment(workshop.Id, participant.Id, now);
            validEnrollmentsToSave.Add(enrollment);

            // Update in-memory state for subsequent rows
            workshopActiveCounts[normalizedCode] = currentActive + 1;
            participantWorkshops.Add((workshop.Id, workshop.Code, workshop.StartDateTime, workshop.EndDateTime));
        }

        // Save all valid enrollments
        foreach (var enrollment in validEnrollmentsToSave)
        {
            await _enrollmentRepository.AddAsync(enrollment, cancellationToken);
        }

        if (validEnrollmentsToSave.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ExcelImportResultDto(
            TotalProcessed: rows.Count,
            EnrolledCount: validEnrollmentsToSave.Count,
            RejectedCount: errors.Count,
            Errors: errors
        );
    }
}
