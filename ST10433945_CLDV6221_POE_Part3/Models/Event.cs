using System.ComponentModel.DataAnnotations;

namespace EventEaseBookingSystem.Models
{
    public class Event
    {
        public int? EventId { get; set; }

        [Required, MaxLength(100)]
        public string? EventName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string? Description { get; set; } = string.Empty;
       
        public DateTime EventDateTime { get; set; }

        public string? ImageUrl { get; set; } = string.Empty;

        public string? EventPath { get; set; } = string.Empty;

        [Required]
        public int? VenueId { get; set; }
        public Venue? Venue { get; set; } = null!;

        [Required]
        public int? EventTypeId { get; set; }  // ✅ Ensure it's nullable with [Required] to trigger MVC validation
        public EventType? EventType { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}