namespace SkillHub.Application.Interfaces;

public record ExcelEnrollmentRow(int RowNumber, string ParticipantEmail, string WorkshopCode);

public interface IExcelReader
{
    IEnumerable<ExcelEnrollmentRow> ReadEnrollmentRows(Stream excelStream);
}
