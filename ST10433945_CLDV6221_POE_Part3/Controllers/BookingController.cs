using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEaseBookingSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EventEaseBookingSystem.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Booking
        public async Task<IActionResult> Index(string search, int? eventTypeId, DateTime? startDate, DateTime? endDate, string venueAvailability)
        {
            var bookingsQuery = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingId.ToString().Contains(search));
            }

            if (eventTypeId.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.Event.EventTypeId == eventTypeId.Value);
            }

            if (startDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate.Date <= endDate.Value.Date);
            }

            var bookings = await bookingsQuery.ToListAsync();

            var allEvents = await _context.Events
                .Select(e => new { e.EventId, e.VenueId, e.EventDateTime })
                .ToListAsync();

            var allBookings = await _context.Bookings
    .Select(b => new { b.BookingId, b.VenueId, b.BookingDate })
    .ToListAsync();

            foreach (var booking in bookings)
            {
                bool eventClash = allEvents.Any(e =>
                    e.VenueId == booking.VenueId &&
                    e.EventDateTime.Date == booking.BookingDate.Date &&
                    e.EventId != booking.EventId);

                bool bookingClash = allBookings.Any(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate.Date == booking.BookingDate.Date &&
                    b.BookingId != booking.BookingId);

                if (booking.Venue != null)
                {
                    booking.Venue.Availability = !(eventClash || bookingClash);
                }
            }


            if (!string.IsNullOrEmpty(venueAvailability))
            {
                bool filterAvailable = venueAvailability == "true";
                bookings = bookings.Where(b => b.Venue?.Availability == filterAvailable).ToList();
            }

            ViewBag.EventTypes = await _context.EventTypes.ToListAsync();
            ViewBag.Venues = await _context.Venues.ToListAsync();
            ViewData["Alert"] = TempData["Alert"];
            ViewData["AlertType"] = TempData["AlertType"];

            return View(bookings);
        }

        // GET: Booking/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,VenueId,BookingDate")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["Alert"] = "✅ Booking created successfully.";
                TempData["AlertType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Booking/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["Alert"] = "✏️ Booking updated successfully.";
                    TempData["AlertType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Bookings.Any(b => b.BookingId == booking.BookingId))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Events = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Booking/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Booking/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            TempData["Alert"] = "🗑️ Booking deleted successfully.";
            TempData["AlertType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // GET: Booking/Consolidated
        public async Task<IActionResult> Consolidated(string search)
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                bookings = bookings.Where(b =>
                    b.BookingId.ToString().Contains(search) ||
                    b.Event.EventName.Contains(search));
            }

            ViewData["Alert"] = TempData["Alert"];
            ViewData["AlertType"] = TempData["AlertType"];

            return View(await bookings.ToListAsync());
        }
    }
}
