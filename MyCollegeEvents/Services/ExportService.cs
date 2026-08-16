using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using MyCollegeEvents.Models;
using System.Text;

namespace MyCollegeEvents.Services
{
    public class ExportService : IExportService
    {
        public byte[] ExportToExcel(List<Participant> participants)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("المشاركات");

            // Headers
            worksheet.Cell(1, 1).Value = "الاسم";
            worksheet.Cell(1, 2).Value = "الرقم الجامعي";
            worksheet.Cell(1, 3).Value = "القسم";
            worksheet.Cell(1, 4).Value = "الفعالية";
            worksheet.Cell(1, 5).Value = "البريد الإلكتروني";
            worksheet.Cell(1, 6).Value = "شاركت سابقاً";
            worksheet.Cell(1, 7).Value = "تريد شهادة";
            worksheet.Cell(1, 8).Value = "موافقة المشرفة";
            worksheet.Cell(1, 9).Value = "تاريخ التسجيل";

            // Style headers
            var headerRange = worksheet.Range(1, 1, 1, 9);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Data
            for (int i = 0; i < participants.Count; i++)
            {
                var participant = participants[i];
                var row = i + 2;

                worksheet.Cell(row, 1).Value = participant.Name;
                worksheet.Cell(row, 2).Value = participant.UniversityID;
                worksheet.Cell(row, 3).Value = participant.Department;
                worksheet.Cell(row, 4).Value = participant.Event?.Title ?? "";
                worksheet.Cell(row, 5).Value = participant.Email;
                worksheet.Cell(row, 6).Value = participant.AttendedBefore ? "نعم" : "لا";
                worksheet.Cell(row, 7).Value = participant.WantCertificate ? "نعم" : "لا";
                worksheet.Cell(row, 8).Value = participant.Approved ? "موافق" : "في الانتظار";
                worksheet.Cell(row, 9).Value = participant.RegistrationDate.ToString("dd/MM/yyyy");
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportToPdf(List<Participant> participants)
        {
            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate());
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var title = new Paragraph("قائمة المشاركات في الفعاليات", titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(title);
            document.Add(new Paragraph(" "));

            // Table
            var table = new PdfPTable(9)
            {
                WidthPercentage = 100
            };

            // Headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            table.AddCell(new PdfPCell(new Phrase("الاسم", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("الرقم الجامعي", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("القسم", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("الفعالية", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("البريد", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("شاركت سابقاً", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("تريد شهادة", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("الموافقة", headerFont)));
            table.AddCell(new PdfPCell(new Phrase("تاريخ التسجيل", headerFont)));

            // Data
            var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var participant in participants)
            {
                table.AddCell(new PdfPCell(new Phrase(participant.Name, dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.UniversityID, dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.Department, dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.Event?.Title ?? "", dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.Email, dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.AttendedBefore ? "نعم" : "لا", dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.WantCertificate ? "نعم" : "لا", dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.Approved ? "موافق" : "في الانتظار", dataFont)));
                table.AddCell(new PdfPCell(new Phrase(participant.RegistrationDate.ToString("dd/MM/yyyy"), dataFont)));
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }
    }
}
