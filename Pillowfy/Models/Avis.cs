using System.ComponentModel.DataAnnotations;

namespace Pillowfy.Models
{
    public class Avis
    {
        public int Id { get; set; }

        [Required]
        [Range(1, 5)]
        public int Note { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(500)]
        public string Commentaire { get; set; } = string.Empty;

        public DateTime DateAvis { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;
    }
}