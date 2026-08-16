using Microsoft.Data.SqlClient;
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
            
            // إنشاء مجلد النسخ الاحتياطية إذا لم يكن موجوداً
            if (!Directory.Exists(_backupDirectory))
            {
                Directory.CreateDirectory(_backupDirectory);
            }
        }

        public async Task<bool> CreateBackupAsync(string? backupPath = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = connectionBuilder.InitialCatalog;

                if (string.IsNullOrEmpty(backupPath))
                {
                    backupPath = GetDefaultBackupPath();
                }

                var backupDirectory = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                // تحويل المسار إلى مسار مطلق
                backupPath = Path.GetFullPath(backupPath);

                // التحقق من صلاحيات الكتابة
                if (!HasWritePermission(backupDirectory))
                {
                    throw new UnauthorizedAccessException($"لا توجد صلاحيات كتابة في المجلد: {backupDirectory}");
                }

                // استعلام النسخ الاحتياطي المحسن
                var backupQuery = $@"
                    DECLARE @BackupPath NVARCHAR(500) = N'{backupPath.Replace("'", "''")}'

                    -- التحقق من وجود قاعدة البيانات
                    IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'{databaseName}')
                    BEGIN
                        RAISERROR('قاعدة البيانات غير موجودة', 16, 1)
                        RETURN
                    END

                    -- إنشاء النسخة الاحتياطية
                    BACKUP DATABASE [{databaseName}]
                    TO DISK = @BackupPath
                    WITH
                        FORMAT,
                        INIT,
                        NAME = N'College Events Database Backup - {DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        DESCRIPTION = N'نسخة احتياطية لقاعدة بيانات فعاليات الكلية',
                        SKIP,
                        NOREWIND,
                        NOUNLOAD,
                        COMPRESSION,
                        STATS = 5";

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(backupQuery, connection);
                command.CommandTimeout = 600; // 10 دقائق

                await command.ExecuteNonQueryAsync();

                // التحقق من إنشاء الملف
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("فشل في إنشاء ملف النسخة الاحتياطية");
                }

                Console.WriteLine($"تم إنشاء النسخة الاحتياطية بنجاح: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل في إنشاء النسخة الاحتياطية: {ex.Message}");
                Console.WriteLine($"تفاصيل الخطأ: {ex.StackTrace}");
                return false;
            }
        }

        private bool HasWritePermission(string? directoryPath)
        {
            try
            {
                if (string.IsNullOrEmpty(directoryPath))
                    return false;

                var testFile = Path.Combine(directoryPath, "test_write_permission.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RestoreBackupAsync(string backupPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                // التحقق من نوع الملف
                if (backupPath.EndsWith(".sql"))
                {
                    // استعادة النسخة المبسطة
                    return await RestoreSimpleBackupAsync(backupPath, connectionString);
                }
                else
                {
                    // استعادة النسخة العادية
                    return await RestoreFullBackupAsync(backupPath, connectionString);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل في استعادة النسخة الاحتياطية: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RestoreSimpleBackupAsync(string backupPath, string connectionString)
        {
            try
            {
                // قراءة ملف SQL
                var sqlStatements = await File.ReadAllLinesAsync(backupPath);

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // حذف البيانات الحالية
                var clearQuery = @"
                    DELETE FROM Participants;
                    DELETE FROM Events;
                    DBCC CHECKIDENT ('Events', RESEED, 0);
                    DBCC CHECKIDENT ('Participants', RESEED, 0);";

                using var clearCommand = new SqlCommand(clearQuery, connection);
                await clearCommand.ExecuteNonQueryAsync();

                // تنفيذ استعلامات الاستعادة
                foreach (var statement in sqlStatements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        using var command = new SqlCommand(statement, connection);
                        await command.ExecuteNonQueryAsync();
                    }
                }

                Console.WriteLine($"تم استعادة النسخة المبسطة بنجاح: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل في استعادة النسخة المبسطة: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RestoreFullBackupAsync(string backupPath, string connectionString)
        {
            try
            {
                var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = connectionBuilder.InitialCatalog;

                // تغيير الاتصال إلى master database
                connectionBuilder.InitialCatalog = "master";
                var masterConnectionString = connectionBuilder.ConnectionString;

                var restoreQuery = $@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [{databaseName}]
                    FROM DISK = '{backupPath.Replace("'", "''")}'
                    WITH REPLACE, STATS = 5;
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(restoreQuery, connection);
                command.CommandTimeout = 600;

                await command.ExecuteNonQueryAsync();

                Console.WriteLine($"تم استعادة النسخة الكاملة بنجاح: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل في استعادة النسخة الكاملة: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetAvailableBackupsAsync()
        {
            try
            {
                var allBackupFiles = new List<string>();

                // إضافة ملفات .bak (النسخ الاحتياطية العادية)
                if (Directory.Exists(_backupDirectory))
                {
                    var bakFiles = Directory.GetFiles(_backupDirectory, "*.bak");
                    allBackupFiles.AddRange(bakFiles);

                    // إضافة ملفات .sql (النسخ الاحتياطية المبسطة)
                    var sqlFiles = Directory.GetFiles(_backupDirectory, "SimpleBackup_*.sql");
                    allBackupFiles.AddRange(sqlFiles);
                }

                var sortedFiles = allBackupFiles
                    .OrderByDescending(f => File.GetCreationTime(f))
                    .ToList();

                return await Task.FromResult(sortedFiles);
            }
            catch (Exception)
            {
                return new List<string>();
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

        // طريقة بديلة للنسخ الاحتياطي باستخدام Entity Framework (للحالات الطارئة)
        public async Task<bool> CreateSimpleBackupAsync()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(_backupDirectory, $"SimpleBackup_{timestamp}.sql");

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = connectionBuilder.InitialCatalog;

                var exportQuery = $@"
                    -- تصدير بيانات الفعاليات
                    SELECT 'INSERT INTO Events (Title, Description, Date, CreatedBy, CreatedDate) VALUES (' +
                           '''' + REPLACE(Title, '''', '''''') + ''', ' +
                           '''' + REPLACE(Description, '''', '''''') + ''', ' +
                           '''' + CONVERT(VARCHAR, Date, 120) + ''', ' +
                           '''' + REPLACE(CreatedBy, '''', '''''') + ''', ' +
                           '''' + CONVERT(VARCHAR, CreatedDate, 120) + ''')'
                    FROM Events

                    UNION ALL

                    -- تصدير بيانات المشاركات
                    SELECT 'INSERT INTO Participants (Name, UniversityID, Department, EventID, AttendedBefore, WantCertificate, Email, Approved, RegistrationDate) VALUES (' +
                           '''' + REPLACE(Name, '''', '''''') + ''', ' +
                           '''' + REPLACE(UniversityID, '''', '''''') + ''', ' +
                           '''' + REPLACE(Department, '''', '''''') + ''', ' +
                           CAST(EventID AS VARCHAR) + ', ' +
                           CASE WHEN AttendedBefore = 1 THEN '1' ELSE '0' END + ', ' +
                           CASE WHEN WantCertificate = 1 THEN '1' ELSE '0' END + ', ' +
                           '''' + REPLACE(Email, '''', '''''') + ''', ' +
                           CASE WHEN Approved = 1 THEN '1' ELSE '0' END + ', ' +
                           '''' + CONVERT(VARCHAR, RegistrationDate, 120) + ''')'
                    FROM Participants";

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(exportQuery, connection);
                using var reader = await command.ExecuteReaderAsync();

                var sqlStatements = new List<string>();
                while (await reader.ReadAsync())
                {
                    sqlStatements.Add(reader.GetString(0));
                }

                await File.WriteAllLinesAsync(backupPath, sqlStatements);

                Console.WriteLine($"تم إنشاء نسخة احتياطية مبسطة: {backupPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"فشل في إنشاء النسخة الاحتياطية المبسطة: {ex.Message}");
                return false;
            }
        }
    }
}
