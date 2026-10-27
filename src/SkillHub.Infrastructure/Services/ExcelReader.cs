using OfficeOpenXml;
using SkillHub.Application.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class ExcelReader : IExcelReader
{
    static ExcelReader()
    {
        ExcelPackage.License.SetNonCommercialPersonal("SkillHub");
    }

    public IEnumerable<ExcelEnrollmentRow> ReadEnrollmentRows(Stream excelStream)
    {
        var result = new List<ExcelEnrollmentRow>();

        using var package = new ExcelPackage(excelStream);
        if (package.Workbook.Worksheets.Count == 0)
            return result;

        // Try to find the "Enrollments" worksheet, or fallback to the first worksheet
        var worksheet = package.Workbook.Worksheets["Enrollments"] ?? package.Workbook.Worksheets[0];
        if (worksheet.Dimension == null)
            return result;

        int rowCount = worksheet.Dimension.Rows;
        int colCount = worksheet.Dimension.Columns;

        // Locate header columns
        int emailCol = -1;
        int codeCol = -1;

        for (int col = 1; col <= colCount; col++)
        {
            var header = worksheet.Cells[1, col].Text?.Trim().ToLowerInvariant();
            if (header == "participantemail" || header == "email" || header == "correo" || header == "participante")
            {
                emailCol = col;
            }
            else if (header == "workshopcode" || header == "code" || header == "codigo" || header == "codigotaller")
            {
                codeCol = col;
            }
        }

        // If not found by header name, default to 1st and 2nd column
        if (emailCol == -1) emailCol = 1;
        if (codeCol == -1) codeCol = 2;

        for (int row = 2; row <= rowCount; row++)
        {
            var email = worksheet.Cells[row, emailCol].Text?.Trim();
            var code = worksheet.Cells[row, codeCol].Text?.Trim();

            // Ignore completely empty rows at the end of the sheet
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(code))
                continue;

            result.Add(new ExcelEnrollmentRow(row, email ?? string.Empty, code ?? string.Empty));
        }

        return result;
    }
}
