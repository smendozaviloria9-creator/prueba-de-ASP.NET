using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

public class WorkshopsController : Controller
{
    private readonly IWorkshopService _workshopService;
    private readonly IEnrollmentService _enrollmentService;

    public WorkshopsController(IWorkshopService workshopService, IEnrollmentService enrollmentService)
    {
        _workshopService = workshopService;
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var workshops = await _workshopService.GetAvailableWorkshopsAsync();
        return View(workshops);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var workshop = await _workshopService.GetWorkshopByIdAsync(id);
            return View(workshop);
        }
        catch (NotFoundException)
        {
            TempData["Error"] = "El taller solicitado no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Participant)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int workshopId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        try
        {
            await _enrollmentService.EnrollAsync(userId, workshopId);
            TempData["Success"] = "¡Inscripción realizada exitosamente!";
            return RedirectToAction("Mine", "Enrollments");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ocurrió un error al procesar tu inscripción: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
