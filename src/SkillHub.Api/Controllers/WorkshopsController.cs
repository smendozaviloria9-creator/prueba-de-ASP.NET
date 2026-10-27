using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Application.DTOs.Workshops;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public class WorkshopsController : ControllerBase
{
    private readonly IWorkshopService _workshopService;

    public WorkshopsController(IWorkshopService workshopService)
    {
        _workshopService = workshopService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WorkshopDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableWorkshops(CancellationToken cancellationToken)
    {
        var workshops = await _workshopService.GetAvailableWorkshopsAsync(cancellationToken);
        return Ok(workshops);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkshopDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkshopById(int id, CancellationToken cancellationToken)
    {
        var workshop = await _workshopService.GetWorkshopByIdAsync(id, cancellationToken);
        return Ok(workshop);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(typeof(WorkshopDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateWorkshop([FromBody] CreateWorkshopDto request, CancellationToken cancellationToken)
    {
        var workshop = await _workshopService.CreateWorkshopAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetWorkshopById), new { id = workshop.Id }, workshop);
    }

    [HttpGet("{id:int}/enrollments")]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetWorkshopEnrollments(int id, CancellationToken cancellationToken)
    {
        var enrollments = await _workshopService.GetWorkshopEnrollmentsAsync(id, cancellationToken);
        return Ok(enrollments);
    }
}
