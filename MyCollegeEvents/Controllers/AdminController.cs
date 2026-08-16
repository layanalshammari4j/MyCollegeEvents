using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCollegeEvents.Data;
using MyCollegeEvents.Models;
using MyCollegeEvents.Services;

namespace MyCollegeEvents.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IExportService _exportService;
        private readonly IBackupService _backupService;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IEmailService emailService, IExportService exportService, IBackupService backupService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _exportService = exportService;
            _backupService = backupService;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddEvent()
        {
            return View();
        }

        // تم تعديل هذه الدالة لتكشف لنا سبب الخطأ الحقيقي
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEvent(Event eventModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    eventModel.CreatedDate = DateTime.Now;
                    _context.Events.Add(eventModel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "تم إضافة الفعالية بنجاح!";
                    return RedirectToAction("ViewEvents");
                }
                catch (Exception ex)
                {
                    // هذا التعديل سيظهر رسالة الخطأ التقنية الحقيقية في الموقع
                    ModelState.AddModelError("", $"خطأ تفصيلي: {ex.Message} -- التفاصيل: {ex.InnerException?.Message}");
                }
            }

            return View(eventModel);
        }

        public async Task<IActionResult> ViewEvents()
        {
            var events = await _context.Events
                .Include(e => e.Participants)
                .OrderByDescending(e => e.CreatedDate)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> ViewParticipants(int? eventId, bool? approved, string searchTerm)
        {
            var query = _context.Participants.Include(p => p.Event).AsQueryable();

            if (eventId.HasValue)
            {
                query = query.Where(p => p.EventID == eventId.Value);
            }

            if (approved.HasValue)
            {
                query = query.Where(p => p.Approved == approved.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Email.Contains(searchTerm));
            }

            var participants = await query
                .OrderByDescending(p => p.RegistrationDate)
                .ToListAsync();

            var events = await _context.Events.ToListAsync();
            ViewBag.Events = events;
            ViewBag.SelectedEventId = eventId;
            ViewBag.SelectedApproved = approved;
            ViewBag.SearchTerm = searchTerm;

            return View(participants);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveParticipant(int id)
        {
            var participant = await _context.Participants
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.ParticipantID == id);

            if (participant != null)
            {
                participant.Approved = true;
                await _context.SaveChangesAsync();
                await _emailService.SendApprovalEmailAsync(participant);
                TempData["SuccessMessage"] = "تم قبول المشاركة وإرسال بريد التأكيد!";
            }

            return RedirectToAction("ViewParticipants");
        }

        [HttpPost]
        public async Task<IActionResult> RejectParticipant(int id)
        {
            var participant = await _context.Participants.FindAsync(id);
            if (participant != null)
            {
                _context.Participants.Remove(participant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم رفض المشاركة!";
            }

            return RedirectToAction("ViewParticipants");
        }

        public async Task<IActionResult> ExportExcel(int? eventId)
        {
            var query = _context.Participants.Include(p => p.Event).AsQueryable();
            if (eventId.HasValue) query = query.Where(p => p.EventID == eventId.Value);

            var participants = await query.ToListAsync();
            var excelData = _exportService.ExportToExcel(participants);
            var fileName = eventId.HasValue 
                ? $"مشاركات_الفعالية_{eventId}_{DateTime.Now:yyyyMMdd}.xlsx"
                : $"جميع_المشاركات_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> ExportPdf(int? eventId)
        {
            var query = _context.Participants.Include(p => p.Event).AsQueryable();
            if (eventId.HasValue) query = query.Where(p => p.EventID == eventId.Value);

            var participants = await query.ToListAsync();
            var pdfData = _exportService.ExportToPdf(participants);
            var fileName = eventId.HasValue 
                ? $"مشاركات_الفعالية_{eventId}_{DateTime.Now:yyyyMMdd}.pdf"
                : $"جميع_المشاركات_{DateTime.Now:yyyyMMdd}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }

        public async Task<IActionResult> SendEmails()
        {
            var events = await _context.Events.ToListAsync();
            ViewBag.Events = events;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmails(int? eventId, string subject, string message, bool approvedOnly = true)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("", "الموضوع والرسالة مطلوبان");
                ViewBag.Events = await _context.Events.ToListAsync();
                return View();
            }

            var query = _context.Participants.Include(p => p.Event).AsQueryable();
            if (eventId.HasValue) query = query.Where(p => p.EventID == eventId.Value);
            if (approvedOnly) query = query.Where(p => p.Approved);

            var participants = await query.ToListAsync();
            if (participants.Any())
            {
                var success = await _emailService.SendBulkEmailsAsync(participants, subject, message);
                if (success) TempData["SuccessMessage"] = $"تم إرسال البريد الإلكتروني إلى {participants.Count} مشاركة بنجاح!";
                else TempData["ErrorMessage"] = "حدث خطأ أثناء إرسال البريد الإلكتروني";
            }
            else
            {
                TempData["ErrorMessage"] = "لا توجد مشاركات مطابقة للمعايير المحددة";
            }

            return RedirectToAction("SendEmails");
        }

        public async Task<IActionResult> BackupRestore()
        {
            var backups = await _backupService.GetAvailableBackupsAsync();
            ViewBag.Backups = backups.Select(path => new {
                Path = path,
                FileName = Path.GetFileName(path),
                CreatedDate = System.IO.File.GetCreationTime(path),
                Size = new FileInfo(path).Length
            }).ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBackup()
        {
            try
            {
                var success = await _backupService.CreateBackupAsync();
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "تم إنشاء النسخة الاحتياطية بنجاح!" : "فشل في إنشاء النسخة الاحتياطية.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = $"حدث خطأ: {ex.Message}"; }
            return RedirectToAction("BackupRestore");
        }

        [HttpPost]
        public async Task<IActionResult> RestoreBackup(string backupPath)
        {
            try
            {
                var success = await _backupService.RestoreBackupAsync(backupPath);
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "تم استعادة النسخة الاحتياطية بنجاح!" : "فشل في استعادة النسخة الاحتياطية";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = $"حدث خطأ: {ex.Message}"; }
            return RedirectToAction("BackupRestore");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBackup(string backupPath)
        {
            try
            {
                var success = await _backupService.DeleteBackupAsync(backupPath);
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "تم حذف النسخة الاحتياطية بنجاح!" : "فشل في حذف النسخة الاحتياطية";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = $"حدث خطأ: {ex.Message}"; }
            return RedirectToAction("BackupRestore");
        }

        public IActionResult DownloadBackup(string backupPath)
        {
            if (string.IsNullOrEmpty(backupPath) || !System.IO.File.Exists(backupPath)) return RedirectToAction("BackupRestore");
            return File(System.IO.File.ReadAllBytes(backupPath), "application/octet-stream", Path.GetFileName(backupPath));
        }

        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var testEmail = _configuration["EmailSettings:AdminEmail"] ?? "mona2022project@gmail.com";
                var testParticipant = new Participant { Name = "اختبار النظام", Email = testEmail, Event = new Event { Title = "اختبار إرسال البريد الإلكتروني" } };
                var success = await _emailService.SendConfirmationEmailAsync(testParticipant);
                TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? "تم إرسال بريد الاختبار بنجاح." : "فشل في إرسال بريد الاختبار.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = $"خطأ: {ex.Message}"; }
            return RedirectToAction("EmailSettings");
        }

        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null) return RedirectToAction("ViewEvents");
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, Event eventItem)
        {
            if (id != eventItem.EventID) return RedirectToAction("ViewEvents");
            if (ModelState.IsValid)
            {
                _context.Update(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تحديث الفعالية بنجاح!";
                return RedirectToAction("ViewEvents");
            }
            return View(eventItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null && !await _context.Participants.AnyAsync(p => p.EventID == id))
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم حذف الفعالية بنجاح!";
            }
            return RedirectToAction("ViewEvents");
        }

        public IActionResult EmailSettings()
        {
            ViewBag.CurrentSettings = new {
                TestMode = bool.Parse(_configuration["EmailSettings:TestMode"] ?? "true"),
                SenderEmail = _configuration["EmailSettings:SenderEmail"] ?? "",
                SmtpServer = _configuration["EmailماءServer"] ?? "",
                SmtpPort = _configuration["EmailSettings:SmtpPort"] ?? "",
                AdminEmail = _configuration["EmailSettings:AdminEmail"] ?? ""
            };
            return View();
        }
    }
}