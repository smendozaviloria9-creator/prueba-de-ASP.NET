using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Participant)]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto request, CancellationToken cancellationToken)
    {
        var participantId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(participantId))
            return Unauthorized();

        var enrollment = await _enrollmentService.EnrollAsync(participantId, request.WorkshopId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, enrollment);
    }

    [HttpGet("mine")]
    [Authorize(Roles = UserRoles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var participantId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(participantId))
            return Unauthorized();

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(participantId, cancellationToken);
        return Ok(enrollments);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelEnrollment(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var isEmployee = User.IsInRole(UserRoles.Employee) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Administrador);
        var enrollment = await _enrollmentService.CancelEnrollmentAsync(id, userId, isEmployee, cancellationToken);

        return Ok(enrollment);
    }

    [HttpPost("import")]
    [Authorize(Roles = $"{UserRoles.Employee},{UserRoles.Admin},Administrador")]
    [ProducesResponseType(typeof(ExcelImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ImportEnrollments(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "An Excel file (.xlsx) must be provided." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only Excel files (.xlsx) are supported." });

        using var stream = file.OpenReadStream();
        var result = await _enrollmentService.ImportFromExcelAsync(stream, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{id:int}/certificate")]
    [Authorize]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadCertificate(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var isEmployee = User.IsInRole(UserRoles.Employee) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Administrador);
        var pdfBytes = await _enrollmentService.GenerateCertificatePdfAsync(id, userId, isEmployee, cancellationToken);

        return File(pdfBytes, "application/pdf", $"Constancia_Inscripcion_{id}.pdf");
    }
}
