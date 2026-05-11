namespace Pillowfy.DTOs
{
   
        public class OwnerReservatioDetailDto
        {
            public int Id { get; set; }
            public string ClientName { get; set; }
            public string ClientEmail { get; set; }      // ← ajouter si manquant
            public string ChambreName { get; set; }
            public string HotelName { get; set; }
            public DateTime CheckIn { get; set; }
            public DateTime CheckOut { get; set; }
            public decimal TotalPrice { get; set; }
            public string Status { get; set; }
            public int NumberOfGuests { get; set; }       // ← ajouter si manquant
        }
    
}
