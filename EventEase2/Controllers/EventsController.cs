using EventEase.Models;
using EventEase2.Data;
using EventEase2.Models;
using EventEase2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase2.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEaseContext _context;
        private readonly BlobStorageService _blobService;

        public EventsController(EventEaseContext context, BlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            var events = _context.Events.Include(e => e.EventType);
            return View(await events.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "TypeName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,StartTime,EndTime,Description,EventTypeId")] Event @event, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = $"event-{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    @event.ImageUrl = await _blobService.UploadFileAsync(imageFile, fileName);
                }
                else
                {
                    @event.ImageUrl = "https://via.placeholder.com/400x300?text=Event";
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,StartTime,EndTime,Description,EventTypeId,ImageUrl")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("placeholder"))
                        {
                            await _blobService.DeleteFileAsync(@event.ImageUrl);
                        }

                        var fileName = $"event-{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                        @event.ImageUrl = await _blobService.UploadFileAsync(imageFile, fileName);
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventTypeId"] = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);

            var hasBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings associated with it.";
                return RedirectToAction(nameof(Index));
            }

            if (@event != null)
            {
                if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("placeholder"))
                {
                    await _blobService.DeleteFileAsync(@event.ImageUrl);
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
