using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;

namespace SkillHub.Web.Controllers;

[Authorize(Roles = $"{UserRoles.Employee},{UserRoles.Admin},Administrador")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> TopWorkshops([FromQuery] int limit = 15)
    {
        var topWorkshops = await _reportService.GetTopWorkshopsAsync(limit);
        return View(topWorkshops);
    }
}
