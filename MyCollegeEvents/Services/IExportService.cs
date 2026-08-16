using MyCollegeEvents.Models;

namespace MyCollegeEvents.Services
{
    public interface IExportService
    {
        byte[] ExportToExcel(List<Participant> participants);
        byte[] ExportToPdf(List<Participant> participants);
    }
}
