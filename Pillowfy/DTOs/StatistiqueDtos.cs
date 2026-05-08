namespace Pillowfy.DTOs
{
    public class DashboardDto
    {
        public int TotalReservations { get; set; }
        public int ReservationsConfirmees { get; set; }
        public int ReservationsAnnulees { get; set; }
        public decimal RevenuTotal { get; set; }
        public double TauxOccupation { get; set; }
        public int TotalHotels { get; set; }
        public int TotalChambres { get; set; }
        public int TotalClients { get; set; }
        public int TotalAvis { get; set; }
        public double NoteMoyenneGlobale { get; set; }
        public List<ReservationsParMoisDto> ReservationsParMois { get; set; } = new();
        public List<HotelPopulaireDto> HotelsLesPlusReserves { get; set; } = new();
    }

    public class ReservationsParMoisDto
    {
        public string Mois { get; set; } = string.Empty;
        public int NombreReservations { get; set; }
        public decimal Revenu { get; set; }
    }

    public class HotelPopulaireDto
    {
        public string HotelName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int NombreReservations { get; set; }
        public double NoteMoyenne { get; set; }
    }
}