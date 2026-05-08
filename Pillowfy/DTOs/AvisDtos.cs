using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs
{
    public class AvisCreateDto
    {
        [Required]
        [Range(1, 5)]
        public int Note { get; set; }

        [Required]
        [MinLength(10)]
        [MaxLength(500)]
        public string Commentaire { get; set; } = string.Empty;

        [Required]
        public int HotelId { get; set; }
    }

    public class AvisResponseDto
    {
        public int Id { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public DateTime DateAvis { get; set; }
        public string? AuteurNom { get; set; }
        public int HotelId { get; set; }
        public string? HotelName { get; set; }
    }
}