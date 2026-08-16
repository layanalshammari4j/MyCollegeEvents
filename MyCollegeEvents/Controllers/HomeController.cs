using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCollegeEvents.Data;
using MyCollegeEvents.Models;

namespace MyCollegeEvents.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // عرض الفعاليات المتاحة للطالبات (جميع الفعاليات للاختبار)
                var events = await _context.Events
                    .OrderBy(e => e.Date)
                    .ToListAsync();

                _logger.LogInformation($"تم العثور على {events.Count} فعالية");

                return View(events);
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في تحميل الفعاليات: {ex.Message}");
                return View(new List<Event>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
