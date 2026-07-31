using EventEase2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(EventEaseContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var venueCount = await _context.Venues.CountAsync();
            var eventCount = await _context.Events.CountAsync();
            var bookingCount = await _context.Bookings.CountAsync();

            ViewBag.VenueCount = venueCount;
            ViewBag.EventCount = eventCount;
            ViewBag.BookingCount = bookingCount;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
