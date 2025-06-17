using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEaseBookingSystem.Models;

namespace EventEaseBookingSystem.Controllers
{
    public class VenueController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VenueController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ✅ List Venues
        public async Task<IActionResult> Index(string search)
        {
            var venues = _context.Venues.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                venues = venues.Where(v => v.VenueName.Contains(search));
            }

            ViewData["Alert"] = TempData["Alert"];
            ViewData["AlertType"] = TempData["AlertType"];

            return View(await venues.ToListAsync());
        }

        // ✅ Venue Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // ✅ Create Venue (GET)
        public IActionResult Create()
        {
            return View();
        }

        // ✅ Create Venue (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueName,Location,Capacity")] Venue venue, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var filePath = Path.Combine(uploads, ImageFile.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }

                    venue.ImageUrl = "/images/" + ImageFile.FileName;
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();

                TempData["Alert"] = "✅ Venue created successfully.";
                TempData["AlertType"] = "success";
                return RedirectToAction(nameof(Index));
            }

            return View(venue);
        }

        // ✅ Edit Venue (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // ✅ Edit Venue (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,Capacity")] Venue venue, IFormFile ImageFile)
        {
            if (id != venue.VenueId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venues.FindAsync(id);
                    if (existingVenue == null)
                        return NotFound();

                    existingVenue.VenueName = venue.VenueName;
                    existingVenue.Location = venue.Location;
                    existingVenue.Capacity = venue.Capacity;

                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        if (!Directory.Exists(uploads))
                            Directory.CreateDirectory(uploads);

                        var filePath = Path.Combine(uploads, ImageFile.FileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        existingVenue.ImageUrl = "/images/" + ImageFile.FileName;
                    }

                    _context.Update(existingVenue);
                    await _context.SaveChangesAsync();

                    TempData["Alert"] = "✅ Venue updated successfully.";
                    TempData["AlertType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venues.Any(v => v.VenueId == venue.VenueId))
                        return NotFound();
                    throw;
                }
            }

            return View(venue);
        }

        // ✅ Delete Venue (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // ✅ Delete Venue (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound();

            if (venue.Bookings.Any())
            {
                TempData["Alert"] = "⚠️ You can not delete a venue with active bookings.";
                TempData["AlertType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["Alert"] = "🗑️ Venue deleted successfully.";
            TempData["AlertType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Check Availability for a venue on a specific date
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int venueId, DateTime date)
        {
            // Check if any event exists on that date for this venue
            bool hasEvent = await _context.Events
                .AnyAsync(e => e.VenueId == venueId && e.EventDateTime.Date == date.Date);

            // Check if any booking exists on that date for this venue
            bool hasBooking = await _context.Bookings
                .AnyAsync(b => b.VenueId == venueId && b.BookingDate.Date == date.Date);

            bool isAvailable = !hasEvent && !hasBooking;
            return Json(new { available = isAvailable });
        }
    }
}
