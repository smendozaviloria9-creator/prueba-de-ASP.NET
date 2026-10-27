using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using Xunit;

namespace SkillHub.Tests;

public class WorkshopDomainTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesWorkshopSuccessfully()
    {
        var now = DateTime.UtcNow;
        var start = now.AddDays(5);
        var end = start.AddHours(4);

        var workshop = new Workshop(
            code: "FOT-001",
            title: "Fotografía con celular",
            description: "Aprende fotografía",
            instructorId: "instructor-1",
            startDateTime: start,
            endDateTime: end,
            capacity: 15,
            currentUtc: now
        );

        Assert.Equal("FOT-001", workshop.Code);
        Assert.Equal("Fotografía con celular", workshop.Title);
        Assert.Equal(15, workshop.Capacity);
        Assert.Equal(15, workshop.GetAvailableCapacity());
        Assert.False(workshop.HasStarted(now));
    }

    [Fact]
    public void Constructor_WithPastStartDate_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var pastStart = now.AddDays(-1);
        var end = now.AddHours(2);

        var ex = Assert.Throws<BusinessRuleException>(() => new Workshop(
            code: "FOT-001",
            title: "Test",
            description: "Test",
            instructorId: "inst-1",
            startDateTime: pastStart,
            endDateTime: end,
            capacity: 10,
            currentUtc: now
        ));

        Assert.Contains("future start date", ex.Message);
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var start = now.AddDays(2);
        var invalidEnd = start.AddHours(-1);

        var ex = Assert.Throws<BusinessRuleException>(() => new Workshop(
            code: "FOT-001",
            title: "Test",
            description: "Test",
            instructorId: "inst-1",
            startDateTime: start,
            endDateTime: invalidEnd,
            capacity: 10,
            currentUtc: now
        ));

        Assert.Contains("after the start date", ex.Message);
    }

    [Fact]
    public void Constructor_WithZeroOrNegativeCapacity_ThrowsBusinessRuleException()
    {
        var now = DateTime.UtcNow;
        var start = now.AddDays(2);
        var end = start.AddHours(2);

        var ex = Assert.Throws<BusinessRuleException>(() => new Workshop(
            code: "FOT-001",
            title: "Test",
            description: "Test",
            instructorId: "inst-1",
            startDateTime: start,
            endDateTime: end,
            capacity: 0,
            currentUtc: now
        ));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void GetAvailableCapacity_AccountsOnlyForActiveEnrollments()
    {
        var now = DateTime.UtcNow;
        var workshop = new Workshop
        {
            Capacity = 5,
            StartDateTime = now.AddDays(2),
            EndDateTime = now.AddDays(2).AddHours(3)
        };

        workshop.Enrollments.Add(new Enrollment { Status = EnrollmentStatus.Active });
        workshop.Enrollments.Add(new Enrollment { Status = EnrollmentStatus.Active });
        workshop.Enrollments.Add(new Enrollment { Status = EnrollmentStatus.Cancelled });

        Assert.Equal(2, workshop.GetActiveEnrollmentsCount());
        Assert.Equal(3, workshop.GetAvailableCapacity());
    }

    [Fact]
    public void OverlapsWith_DetectsScheduleOverlapsCorrectly()
    {
        var baseDate = DateTime.UtcNow.AddDays(5);
        var workshop = new Workshop
        {
            StartDateTime = baseDate.AddHours(8),
            EndDateTime = baseDate.AddHours(12)
        };

        // Complete overlap inside
        Assert.True(workshop.OverlapsWith(baseDate.AddHours(9), baseDate.AddHours(11)));

        // Partial overlap starting earlier
        Assert.True(workshop.OverlapsWith(baseDate.AddHours(7), baseDate.AddHours(10)));

        // Partial overlap ending later
        Assert.True(workshop.OverlapsWith(baseDate.AddHours(11), baseDate.AddHours(14)));

        // Completely outside before
        Assert.False(workshop.OverlapsWith(baseDate.AddHours(5), baseDate.AddHours(8)));

        // Completely outside after
        Assert.False(workshop.OverlapsWith(baseDate.AddHours(12), baseDate.AddHours(15)));
    }
}
