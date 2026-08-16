using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyCollegeEvents.Data;
using MyCollegeEvents.Models;
using MyCollegeEvents.Services;

namespace MyCollegeEvents.Controllers
{
    public class ParticipateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public ParticipateController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: /Participate/Events
        public async Task<IActionResult> Events()
        {
            try
            {
                var events = await _context.Events
                    .OrderBy(e => e.Date)
                    .ToListAsync();

                return View(events);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"حدث خطأ في تحميل الفعاليات: {ex.Message}";
                return View(new List<Event>());
            }
        }

        // GET: /Participate/Register
        public async Task<IActionResult> Register(int? eventId)
        {
            // إعداد قائمة الفعاليات المتاحة (جميع الفعاليات للاختبار)
            var events = await _context.Events
                .OrderBy(e => e.Date)
                .ToListAsync();

            ViewBag.Events = new SelectList(events, "EventID", "Title", eventId);

            // إعداد قائمة الأقسام
            var departments = new List<string>
            {
                "علوم الحاسب",
                "تقنية المعلومات",
                "الهندسة",
                "إدارة الأعمال",
                "التصميم الجرافيكي",
                "الطب",
                "الصيدلة",
                "التمريض"
            };
            ViewBag.Departments = new SelectList(departments);

            var participant = new Participant();
            if (eventId.HasValue)
            {
                participant.EventID = eventId.Value;
            }

            return View(participant);
        }

        // POST: /Participate/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Participant participant)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // التحقق من عدم وجود تسجيل مسبق
                    var existingParticipant = await _context.Participants
                        .FirstOrDefaultAsync(p => p.Email == participant.Email && p.EventID == participant.EventID);

                    if (existingParticipant != null)
                    {
                        ModelState.AddModelError("", "لقد قمت بالتسجيل في هذه الفعالية مسبقاً");
                        await PrepareViewBag(participant.EventID);
                        return View(participant);
                    }

                    participant.RegistrationDate = DateTime.Now;
                    participant.Approved = false;

                    _context.Participants.Add(participant);
                    await _context.SaveChangesAsync();

                    // تحميل بيانات الفعالية لإرسال البريد
                    participant.Event = await _context.Events.FindAsync(participant.EventID);

                    // إرسال بريد التأكيد
                    await _emailService.SendConfirmationEmailAsync(participant);

                    TempData["SuccessMessage"] = "تم التسجيل بنجاح! سيتم مراجعة طلبك وإرسال تأكيد عبر البريد الإلكتروني.";
                    TempData["ParticipantName"] = participant.Name;
                    TempData["EventTitle"] = participant.Event?.Title;

                    return RedirectToAction("ThankYou");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء التسجيل. يرجى المحاولة مرة أخرى.");
                }
            }

            await PrepareViewBag(participant.EventID);
            return View(participant);
        }

        // GET: /Participate/ThankYou
        public IActionResult ThankYou()
        {
            ViewBag.ParticipantName = TempData["ParticipantName"];
            ViewBag.EventTitle = TempData["EventTitle"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View();
        }

        private async Task PrepareViewBag(int selectedEventId = 0)
        {
            var events = await _context.Events
                .OrderBy(e => e.Date)
                .ToListAsync();

            ViewBag.Events = new SelectList(events, "EventID", "Title", selectedEventId);

            var departments = new List<string>
            {
                "علوم الحاسب",
                "تقنية المعلومات",
                "الهندسة",
                "إدارة الأعمال",
                "التصميم الجرافيكي",
                "الطب",
                "الصيدلة",
                "التمريض"
            };
            ViewBag.Departments = new SelectList(departments);
        }
    }
}
