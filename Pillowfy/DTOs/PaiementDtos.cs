using Pillowfy.Models;
using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs
{
    public class PaiementCreateDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public MethodePaiement Methode { get; set; }
    }

    public class PaiementResponseDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public DateTime DatePaiement { get; set; }
        public string Methode { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int ReservationId { get; set; }
        public string? HotelName { get; set; }
        public string? ChambreName { get; set; }
    }
}