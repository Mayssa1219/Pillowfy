using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs
{
    public class ReservationCreateDto
    {
        [Required]
        public int ChambreId { get; set; }

        [Required]
        public DateTime CheckIn { get; set; }

        [Required]
        public DateTime CheckOut { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }
        // Points à utiliser (optionnel, 0 par défaut)
        public int PointsUtilises { get; set; } = 0;
    }
}
