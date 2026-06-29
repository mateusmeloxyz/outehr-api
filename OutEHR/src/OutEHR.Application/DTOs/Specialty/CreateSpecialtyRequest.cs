using System.ComponentModel.DataAnnotations;

namespace OutEHR.Application.DTOs.Specialty;

public class CreateSpecialtyRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 480)]
    public int DefaultSlotDurationMinutes { get; set; }
}
