using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using Xunit;

namespace SkillHub.Tests;

public class EnrollmentDomainTests
{
    [Fact]
    public void Cancel_ByParticipant_MoreThan24HoursBeforeStart_Succeeds()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Id = 1,
            Code = "FOT-001",
            StartDateTime = now.AddHours(25), // 25 hours before start
            EndDateTime = now.AddHours(29)
        };

        var enrollment = new Enrollment
        {
            Id = 10,
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = "part-1",
            Status = EnrollmentStatus.Active
        };

        enrollment.Cancel(cancelledById: "part-1", isEmployee: false, currentUtc: now);

        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal("part-1", enrollment.CancelledById);
        Assert.Equal(now, enrollment.CancelledAt);
    }

    [Fact]
    public void Cancel_ByParticipant_LessThan24HoursBeforeStart_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Id = 1,
            Code = "FOT-001",
            StartDateTime = now.AddHours(23), // 23 hours before start (< 24h)
            EndDateTime = now.AddHours(27)
        };

        var enrollment = new Enrollment
        {
            Id = 10,
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = "part-1",
            Status = EnrollmentStatus.Active
        };

        var ex = Assert.Throws<BusinessRuleException>(() =>
            enrollment.Cancel(cancelledById: "part-1", isEmployee: false, currentUtc: now));

        Assert.Contains("at least 24 hours before", ex.Message);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
    }

    [Fact]
    public void Cancel_ByEmployee_LessThan24HoursBeforeStart_Succeeds()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Id = 1,
            Code = "FOT-001",
            StartDateTime = now.AddHours(2), // 2 hours before start
            EndDateTime = now.AddHours(6)
        };

        var enrollment = new Enrollment
        {
            Id = 10,
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = "part-1",
            Status = EnrollmentStatus.Active
        };

        enrollment.Cancel(cancelledById: "emp-1", isEmployee: true, currentUtc: now);

        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal("emp-1", enrollment.CancelledById);
        Assert.Equal(now, enrollment.CancelledAt);
    }

    [Fact]
    public void Cancel_WhenWorkshopAlreadyStarted_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Id = 1,
            Code = "FOT-001",
            StartDateTime = now.AddMinutes(-10), // Started 10 minutes ago
            EndDateTime = now.AddHours(2)
        };

        var enrollment = new Enrollment
        {
            Id = 10,
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = "part-1",
            Status = EnrollmentStatus.Active
        };

        var ex = Assert.Throws<BusinessRuleException>(() =>
            enrollment.Cancel(cancelledById: "emp-1", isEmployee: true, currentUtc: now));

        Assert.Contains("already started", ex.Message);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Id = 1,
            Code = "FOT-001",
            StartDateTime = now.AddDays(2),
            EndDateTime = now.AddDays(2).AddHours(4)
        };

        var enrollment = new Enrollment
        {
            Id = 10,
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = "part-1",
            Status = EnrollmentStatus.Cancelled
        };

        var ex = Assert.Throws<BusinessRuleException>(() =>
            enrollment.Cancel(cancelledById: "part-1", isEmployee: false, currentUtc: now));

        Assert.Contains("already cancelled", ex.Message);
    }
}
