using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseBookingSystem.Models;
using EventEaseBookingSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventEaseBookingSystem.Controllers
{
    public class EventController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAzureBlobStorageService _blobService;

        public EventController(AppDbContext context, IAzureBlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index(string search)
        {
            var events = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType) // ✅ Include EventType to populate Type column
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                events = events.Where(e =>
                    e.EventName.Contains(search) ||
                    
                    e.Venue.VenueName.Contains(search) ||
                    e.EventType.Name.Contains(search)); // ✅ Added search for EventType.Name
            }

            ViewData["Alert"] = TempData["Alert"];
            ViewData["AlertType"] = TempData["AlertType"];
            return View(await events.ToListAsync());
        }


        // GET: Event/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.VenueId = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName");
            ViewBag.EventTypeId = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name");
            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event)
        {
            ViewBag.VenueId = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", @event.VenueId);
            ViewBag.EventTypeId = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", @event.EventTypeId);

            if (!ModelState.IsValid)
            {
                return View(@event);
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            TempData["Alert"] = "✅ Event created successfully.";
            TempData["AlertType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // GET: Event/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            ViewBag.VenueId = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", @event.VenueId);
            ViewBag.EventTypeId = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", @event.EventTypeId);

            return View(@event);
        }

        // POST: Event/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.VenueId = new SelectList(await _context.Venues.ToListAsync(), "VenueId", "VenueName", @event.VenueId);
                ViewBag.EventTypeId = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeId", "Name", @event.EventTypeId);
                return View(@event);
            }

            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
                TempData["Alert"] = "✏️ Event updated successfully.";
                TempData["AlertType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Events.Any(e => e.EventId == @event.EventId))
                    return NotFound();
                throw;
            }
        }


        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(m => m.EventId == id);
            return @event == null ? NotFound() : View(@event);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(m => m.EventId == id);
            return @event == null ? NotFound() : View(@event);
        }

        // POST: Event/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.EventId == id);
            if (@event == null) return NotFound();

            if (@event.Bookings.Any())
            {
                TempData["Alert"] = "❌ Cannot delete event with active bookings.";
                TempData["AlertType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            TempData["Alert"] = "🗑️ Event deleted successfully.";
            TempData["AlertType"] = "success";
            return RedirectToAction(nameof(Index));
        }
    }
}
