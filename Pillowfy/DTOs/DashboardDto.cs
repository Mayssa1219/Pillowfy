namespace Pillowfy.DTOs
{
    public class DashboardDto
    {
        public int ReservationsTotal { get; set; }
        public int NuitsTotal { get; set; }
        public int PointsFidelite { get; set; }
        public string Tier { get; set; } = "";
        public string TotalDepense { get; set; } = "";
        public int? NextTierPts { get; set; }       
        public string? NextTierName { get; set; }    
        public int LoyaltyProgress { get; set; }
        public ReservationDto? ActiveReservation { get; set; }
    }

    public class OccupancyDto
    {
        public string HotelName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public int OccupancyRate { get; set; }
    }
}