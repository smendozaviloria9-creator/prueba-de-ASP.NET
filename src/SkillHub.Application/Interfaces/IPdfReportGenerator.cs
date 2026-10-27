using SkillHub.Application.DTOs.Enrollments;

namespace SkillHub.Application.Interfaces;

public interface IPdfReportGenerator
{
    byte[] GenerateEnrollmentCertificate(EnrollmentCertificateData data);
}
