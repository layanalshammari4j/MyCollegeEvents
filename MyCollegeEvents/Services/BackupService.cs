using System.IO.Compression;

namespace MyCollegeEvents.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly string _backupDirectory;

        public BackupService(IConfiguration configuration)
        {
            _configuration = configuration;
            _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");

            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public Task<bool> CreateBackupAsync(string? backupPath = null)
        {
            Console.WriteLine("النسخ الاحتياطي التلقائي غير مفعّل في بيئة الاستضافة الحالية.");
            return Task.FromResult(false);
        }

        public Task<bool> RestoreBackupAsync(string backupPath)
        {
            Console.WriteLine("استعادة النسخة الاحتياطية غير مفعّلة في بيئة الاستضافة الحالية.");
            return Task.FromResult(false);
        }

        public Task<List<string>> GetAvailableBackupsAsync()
        {
            try
            {
                var allBackupFiles = new List<string>();

                if (Directory.Exists(_backupDirectory))
                {
                    var bakFiles = Directory.GetFiles(_backupDirectory, "*.bak");
                    allBackupFiles.AddRange(bakFiles);

                    var sqlFiles = Directory.GetFiles(_backupDirectory, "SimpleBackup_*.sql");
                    allBackupFiles.AddRange(sqlFiles);
                }

                var sortedFiles = allBackupFiles
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                return Task.FromResult(sortedFiles);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<string>());
            }
        }

        public Task<bool> DeleteBackupAsync(string backupPath)
        {
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public string GetDefaultBackupPath()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"CollegeEventsDB_Backup_{timestamp}.bak";
            return Path.Combine(_backupDirectory, fileName);
        }

        public Task<bool> CreateSimpleBackupAsync()
        {
            Console.WriteLine("النسخ الاحتياطي المبسط غير مفعّل في بيئة الاستضافة الحالية.");
            return Task.FromResult(false);
        }
    }
}