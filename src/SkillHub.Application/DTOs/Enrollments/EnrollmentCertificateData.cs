namespace SkillHub.Application.DTOs.Enrollments;

public record EnrollmentCertificateData(
    string OrganizationName,
    string CertificateTitle,
    string ParticipantName,
    string ParticipantEmail,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime WorkshopStartDateTime,
    DateTime WorkshopEndDateTime,
    DateTime EnrolledAt,
    DateTime GeneratedAt
);
