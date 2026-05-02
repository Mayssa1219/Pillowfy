namespace Pillowfy.Models
{
    public class Chambre
    {
        public int Id { get; set; }

        public string Name { get; set; } // ex: "Chambre Deluxe 101"
        public string Description { get; set; }

        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        // Foreign Key
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; }

        // Relations
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
