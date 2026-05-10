using System.ComponentModel.DataAnnotations;

public class AvisUpdateDto
{
    [Required]
    [Range(1, 5)]
    public int Note { get; set; }

    [Required]
    [MinLength(10)]
    [MaxLength(500)]
    public string Commentaire { get; set; } = string.Empty;
}