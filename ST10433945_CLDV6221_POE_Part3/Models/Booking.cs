using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEaseBookingSystem.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        public Event? Event { get; set; } = null!;
        public Venue? Venue { get; set; } = null!;

        [NotMapped]
        public string Date => BookingDate.ToShortDateString();

        [NotMapped]
        public string Time => BookingDate.ToShortTimeString();
    }
}