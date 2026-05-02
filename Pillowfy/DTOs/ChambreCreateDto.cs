namespace Pillowfy.DTOs
{
    public class ChambreCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public int HotelId { get; set; }
    }
}
