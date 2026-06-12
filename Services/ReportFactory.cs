using System.Data;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PersonalFinance.Services
{
    /// <summary>
    /// Паттерн «Фабричный метод». Единый интерфейс генерации отчёта в разных
    /// форматах; конкретный генератор создаётся фабрикой <see cref="ReportFactory"/>.
    /// </summary>
    public interface IReport
    {
        string FileExtension { get; }
        string Description { get; }
        void Generate(DataTable data, string filePath, string title);
    }

    /// <summary>Экспорт отчёта в CSV.</summary>
    public class CsvReport : IReport
    {
        public string FileExtension => ".csv";
        public string Description => "CSV файл";

        public void Generate(DataTable data, string filePath, string title)
        {
            using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
            writer.WriteLine(string.Join(";",
                data.Columns.Cast<DataColumn>().Select(c => Escape(c.ColumnName))));

            foreach (DataRow row in data.Rows)
                writer.WriteLine(string.Join(";",
                    row.ItemArray.Select(item => Escape(item?.ToString() ?? string.Empty))));
        }

        private static string Escape(string value)
        {
            if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }

    /// <summary>
    /// Экспорт отчёта в формат электронной таблицы (SpreadsheetML 2003, .xls),
    /// который открывается Microsoft Excel без дополнительных библиотек.
    /// </summary>
    public class ExcelReport : IReport
    {
        public string FileExtension => ".xls";
        public string Description => "Excel таблица";

        public void Generate(DataTable data, string filePath, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" " +
                          "xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.AppendLine("<Styles>");
            sb.AppendLine("<Style ss:ID=\"hdr\"><Font ss:Bold=\"1\"/>" +
                          "<Interior ss:Color=\"#D9D9D9\" ss:Pattern=\"Solid\"/></Style>");
            sb.AppendLine("</Styles>");
            sb.AppendLine("<Worksheet ss:Name=\"Отчёт\"><Table>");

            sb.AppendLine("<Row>");
            foreach (DataColumn col in data.Columns)
                sb.AppendLine($"<Cell ss:StyleID=\"hdr\"><Data ss:Type=\"String\">{Xml(col.ColumnName)}</Data></Cell>");
            sb.AppendLine("</Row>");

            foreach (DataRow row in data.Rows)
            {
                sb.AppendLine("<Row>");
                foreach (var item in row.ItemArray)
                    sb.AppendLine($"<Cell><Data ss:Type=\"String\">{Xml(item?.ToString() ?? string.Empty)}</Data></Cell>");
                sb.AppendLine("</Row>");
            }

            sb.AppendLine("</Table></Worksheet></Workbook>");
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(false));
        }

        private static string Xml(string value) => System.Security.SecurityElement.Escape(value) ?? string.Empty;
    }

    /// <summary>Экспорт отчёта в PDF средствами библиотеки QuestPDF.</summary>
    public class PdfReport : IReport
    {
        public string FileExtension => ".pdf";
        public string Description => "PDF документ";

        public void Generate(DataTable data, string filePath, string title)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(title).FontSize(16).SemiBold();
                        col.Item().Text($"Сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(def =>
                        {
                            for (int i = 0; i < data.Columns.Count; i++)
                                def.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            foreach (DataColumn col in data.Columns)
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(col.ColumnName).SemiBold();
                        });

                        foreach (DataRow row in data.Rows)
                            foreach (var item in row.ItemArray)
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                     .Padding(4).Text(item?.ToString() ?? string.Empty);
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);
        }
    }

    /// <summary>Фабрика отчётов: создаёт генератор по типу формата.</summary>
    public static class ReportFactory
    {
        public static IReport CreateReport(string type) => type.ToLowerInvariant() switch
        {
            "pdf" => new PdfReport(),
            "excel" => new ExcelReport(),
            "csv" => new CsvReport(),
            _ => throw new ArgumentException($"Неизвестный тип отчёта: {type}")
        };

        public static List<string> GetAvailableFormats() => new() { "PDF", "Excel", "CSV" };
    }
}
