using EventEase.Models;
using EventEase2.Data;
using EventEase2.Models;
using EventEase2.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase2.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventEaseContext _context;

        public BookingsController(EventEaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .ThenInclude(e => e!.EventType)
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    VenueName = b.Venue!.VenueName,
                    VenueLocation = b.Venue.Location,
                    EventName = b.Event!.EventName,
                    EventDate = b.Event.EventDate,
                    StartTime = b.Event.StartTime,
                    EndTime = b.Event.EndTime,
                    EventTypeName = b.Event.EventType!.TypeName
                })
                .ToListAsync();

            return View(bookings);
        }

        public async Task<IActionResult> Search()
        {
            var viewModel = new SearchViewModel
            {
                EventTypes = await _context.EventTypes
                    .Select(et => new SelectListItem { Value = et.EventTypeId.ToString(), Text = et.TypeName })
                    .ToListAsync(),
                Venues = await _context.Venues
                    .Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.VenueName })
                    .ToListAsync(),
                Bookings = new List<BookingViewModel>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchViewModel searchModel)
        {
            var query = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .ThenInclude(e => e!.EventType)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                if (int.TryParse(searchModel.SearchTerm, out int bookingId))
                {
                    query = query.Where(b => b.BookingId == bookingId);
                }
                else
                {
                    query = query.Where(b => b.Event!.EventName.Contains(searchModel.SearchTerm));
                }
            }

            if (searchModel.EventTypeId.HasValue)
            {
                query = query.Where(b => b.Event!.EventTypeId == searchModel.EventTypeId.Value);
            }

            if (searchModel.VenueId.HasValue)
            {
                query = query.Where(b => b.VenueId == searchModel.VenueId.Value);
            }

            if (searchModel.StartDate.HasValue)
            {
                query = query.Where(b => b.BookingDate >= searchModel.StartDate.Value);
            }

            if (searchModel.EndDate.HasValue)
            {
                query = query.Where(b => b.BookingDate <= searchModel.EndDate.Value);
            }

            searchModel.Bookings = await query
                .Select(b => new BookingViewModel
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    VenueName = b.Venue!.VenueName,
                    VenueLocation = b.Venue.Location,
                    EventName = b.Event!.EventName,
                    EventDate = b.Event.EventDate,
                    StartTime = b.Event.StartTime,
                    EndTime = b.Event.EndTime,
                    EventTypeName = b.Event.EventType!.TypeName
                })
                .ToListAsync();

            searchModel.EventTypes = await _context.EventTypes
                .Select(et => new SelectListItem { Value = et.EventTypeId.ToString(), Text = et.TypeName })
                .ToListAsync();

            searchModel.Venues = await _context.Venues
                .Select(v => new SelectListItem { Value = v.VenueId.ToString(), Text = v.VenueName })
                .ToListAsync();

            return View(searchModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName");
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingDate,VenueId,EventId")] Booking booking)
        {
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.VenueId == booking.VenueId && b.BookingDate == booking.BookingDate);

            if (existingBooking != null)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
                ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,BookingDate,VenueId,EventId")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.VenueId == booking.VenueId
                    && b.BookingDate == booking.BookingDate
                    && b.BookingId != booking.BookingId);

            if (existingBooking != null)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
                ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
                ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", booking.VenueId);
            ViewData["EventId"] = new SelectList(await _context.Events.ToListAsync(), "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
