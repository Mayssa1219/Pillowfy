namespace Pillowfy.DTOs
{
    public class OwnerDashboardDto
{
    public int HotelsTotal { get; set; }
    public int ReservationsMois { get; set; }
    public decimal RevenusMois { get; set; }
    public int TauxOccupationMoyen { get; set; }
    public int ReservationsEnAttente { get; set; }

    public List<OwnerReservationDto> DernieresReservations { get; set; } = new();
    public List<OwnerChambreDto> MesChambresDispo { get; set; } = new();
    public List<RevenuMensuelDto> RevenusMensuels { get; set; } = new();
}

public class OwnerReservationDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = "";
    public string ChambreName { get; set; } = "";
    public string HotelName { get; set; } = "";
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "";
}

public class OwnerChambreDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? HotelName { get; set; }
    public string? ImageUrl { get; set; }
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public bool IsAvailable { get; set; }
}

public class RevenuMensuelDto
{
    public string Mois { get; set; } = "";
    public decimal Montant { get; set; }
}

public class OccupancyDto
{
    public string HotelName { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int OccupancyRate { get; set; }
}
}
