namespace Pillowfy.DTOs
{
    public class HotelOwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int ChambresCount { get; set; }
        public int ReservationsCount { get; set; }
        public int OccupancyRate { get; set; }
    }

    public class HotelCreateDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}