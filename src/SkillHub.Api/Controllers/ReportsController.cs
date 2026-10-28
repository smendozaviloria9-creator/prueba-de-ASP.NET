using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Reports;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("top-workshops")]
    [Authorize(Roles = $"{UserRoles.Employee},{UserRoles.Admin},Administrador")]
    [ProducesResponseType(typeof(IEnumerable<TopWorkshopReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopWorkshops([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var topWorkshops = await _reportService.GetTopWorkshopsAsync(limit, cancellationToken);
        return Ok(topWorkshops);
    }
}
