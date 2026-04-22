using System.ComponentModel.DataAnnotations;

namespace Pillowfy.DTOs.Auth
{
	public class RegisterDto
	{
		[Required]
		[StringLength(100)]
		public string FirstName { get; set; }

		[Required]
		[StringLength(100)]
		public string LastName { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 8)]
		public string Password { get; set; }

		[Required]
		[Phone]
		public string PhoneNumber { get; set; }
	}
}