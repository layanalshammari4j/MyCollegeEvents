namespace MyCollegeEvents.Services
{
    public interface IBackupService
    {
        Task<bool> CreateBackupAsync(string? backupPath = null);
        Task<bool> RestoreBackupAsync(string backupPath);
        Task<List<string>> GetAvailableBackupsAsync();
        Task<bool> DeleteBackupAsync(string backupPath);
        string GetDefaultBackupPath();
        Task<bool> CreateSimpleBackupAsync();
    }
}
