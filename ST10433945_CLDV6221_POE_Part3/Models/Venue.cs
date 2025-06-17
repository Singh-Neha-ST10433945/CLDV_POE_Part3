using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseBookingSystem.Models
{
    public class Venue
    {
        public int VenueId { get; set; }

        [Required, MaxLength(100)]
        public string VenueName { get; set; } = string.Empty;

        public string? Location { get; set; }

        public int Capacity { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        [NotMapped]
        public bool Availability { get; set; }


        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
