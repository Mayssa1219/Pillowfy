namespace Pillowfy.DTOs
{
    public class ReservationUpdateDto
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumberOfGuests { get; set; }
    }

    public class AvailabilityResultDto
    {
        public bool Available { get; set; }
        public string? Reason { get; set; }
    }
}
