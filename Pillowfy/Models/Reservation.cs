using Pillowfy.Enums;

namespace Pillowfy.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public int NumberOfGuests { get; set; }

        public decimal TotalPrice { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        
        // Relations
        public int ChambreId { get; set; }
        public Chambre Chambre { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
