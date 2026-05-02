using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ChambreId { get; set; }
        public string ChambreName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }

        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }

        public string Status { get; set; }
    }
}
