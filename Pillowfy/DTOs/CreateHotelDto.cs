using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs.Hotel
{
	public class CreateHotelDto
	{
		[Required]
		[StringLength(200)]
		public string Name { get; set; }

		[Required]
		[StringLength(1000)]
		public string Description { get; set; }

		[Required]
		[StringLength(300)]
		public string Address { get; set; }

		[Required]
		[StringLength(100)]
		public string City { get; set; }

		[Required]
		[StringLength(100)]
		public string Country { get; set; }
	}
}