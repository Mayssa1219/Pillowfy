using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pillowfy.Models
{
    public enum MethodePaiement
    {
        CarteBancaire,
        Virement,
        Especes,
        PayPal
    }

    public enum StatutPaiement
    {
        EnAttente,
        Confirme,
        Echoue,
        Rembourse
    }

    public class Paiement
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Montant { get; set; }

        public DateTime DatePaiement { get; set; } = DateTime.UtcNow;

        [Required]
        public MethodePaiement Methode { get; set; }

        public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;

        [Required]
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;
    }
}