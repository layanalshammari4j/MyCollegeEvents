using MyCollegeEvents.Models;

namespace MyCollegeEvents.Services
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmailAsync(Participant participant);
        Task<bool> SendBulkEmailsAsync(List<Participant> participants, string subject, string message);
        Task<bool> SendApprovalEmailAsync(Participant participant);
    }
}
