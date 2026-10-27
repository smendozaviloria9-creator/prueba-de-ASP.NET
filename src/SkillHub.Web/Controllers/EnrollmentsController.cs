using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

[Authorize(Roles = UserRoles.Participant)]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId);
        return View(enrollments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        try
        {
            await _enrollmentService.CancelEnrollmentAsync(id, userId, isEmployee: false);
            TempData["Success"] = "La inscripción ha sido cancelada exitosamente. Se ha liberado el cupo del taller.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cancelar la inscripción: {ex.Message}";
        }

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        try
        {
            var pdfBytes = await _enrollmentService.GenerateCertificatePdfAsync(id, userId, isEmployee: false);
            return File(pdfBytes, "application/pdf", $"Constancia_Inscripcion_{id}.pdf");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Mine));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar constancia: {ex.Message}";
            return RedirectToAction(nameof(Mine));
        }
    }
}
