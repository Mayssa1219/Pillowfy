namespace Pillowfy.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public decimal Rating { get; set; } = 0;
        public string OwnerId { get; set; }
        public ApplicationUser Owner { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Relations
        public ICollection<Chambre> Chambres { get; set; } = new List<Chambre>();
    }
}