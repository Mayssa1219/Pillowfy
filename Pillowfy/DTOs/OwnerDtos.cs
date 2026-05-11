using Pillowfy.Enums;

namespace Pillowfy.DTOs
{
    // ═══════════════════════════════════════
    // PAIEMENTS
    // ═══════════════════════════════════════

    public class PaiementOwnerDto
    {
        public int Id { get; set; }
        public decimal Montant { get; set; }
        public string DatePaiement { get; set; } = string.Empty;
        public string Methode { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int ReservationId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string ChambreName { get; set; } = string.Empty;
    }

    public class PaiementStatsDto
    {
        public int Confirme { get; set; }
        public int EnAttente { get; set; }
        public int Echoue { get; set; }
        public int Rembourse { get; set; }
        public decimal TotalMois { get; set; }
    }

    public class PaiementsResponseDto
    {
        public PaiementStatsDto Stats { get; set; } = new();

        public List<PaiementOwnerDto> Paiements { get; set; } = new();
    }

    // ═══════════════════════════════════════
    // REVENUS
    // ═══════════════════════════════════════

    public class RevenuMensuelDetailDto
    {
        public string Mois { get; set; } = string.Empty;
        public int MoisIndex { get; set; }
        public int Annee { get; set; }
        public decimal Montant { get; set; }
    }

    public class RevenuParHotelDto
    {
        public string HotelName { get; set; } = string.Empty;
        public decimal Revenus { get; set; }
    }

    public class RevenusResponseDto
    {
        public List<RevenuMensuelDetailDto> RevenusMensuels { get; set; } = new();

        public List<RevenuParHotelDto> ParHotel { get; set; } = new();
    }

    // ═══════════════════════════════════════
    // AVIS
    // ═══════════════════════════════════════

    public class AvisOwnerDto
    {
        public int Id { get; set; }
        public int Note { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public string DateAvis { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
    }
    /* =====================================================
   DTOs
===================================================== */

    public class UpdateOwnerProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class ChangeOwnerPasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

}