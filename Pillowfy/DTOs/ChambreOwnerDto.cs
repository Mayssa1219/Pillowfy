namespace Pillowfy.DTOs
{
    public class ChambreOwnerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int ReservationsCount { get; set; }
    }

    public class ChambreOwnerCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public int HotelId { get; set; }
        public string ImageUrl { get; set; }
    }
}
