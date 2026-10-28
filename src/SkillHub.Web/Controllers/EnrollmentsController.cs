using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

[Authorize]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Participant)]
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
    public async Task<IActionResult> Cancel(int id, string? returnUrl = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var isEmployee = User.IsInRole(UserRoles.Employee) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Administrador);

        try
        {
            await _enrollmentService.CancelEnrollmentAsync(id, userId, isEmployee);
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

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (isEmployee)
            return RedirectToAction("Index", "Workshops");

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var isEmployee = User.IsInRole(UserRoles.Employee) || User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Administrador);

        try
        {
            var pdfBytes = await _enrollmentService.GenerateCertificatePdfAsync(id, userId, isEmployee);
            return File(pdfBytes, "application/pdf", $"Constancia_Inscripcion_{id}.pdf");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Workshops");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar constancia: {ex.Message}";
            return RedirectToAction("Index", "Workshops");
        }
    }

    // Admin/Employee: Excel Import View
    [HttpGet]
    [Authorize(Roles = $"{UserRoles.Employee},{UserRoles.Admin},Administrador")]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Employee},{UserRoles.Admin},Administrador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Por favor selecciona un archivo Excel (.xlsx).");
            return View();
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Solo se admiten archivos en formato Excel (.xlsx).");
            return View();
        }

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _enrollmentService.ImportFromExcelAsync(stream);
            ViewBag.Result = result;
            TempData["Success"] = $"Proceso finalizado: {result.EnrolledCount} inscripciones exitosas de {result.TotalProcessed} procesadas.";
            return View();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al importar el archivo: {ex.Message}");
            return View();
        }
    }
}
