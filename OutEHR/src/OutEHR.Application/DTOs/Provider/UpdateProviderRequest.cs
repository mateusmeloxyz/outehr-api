using System.ComponentModel.DataAnnotations;

namespace OutEHR.Application.DTOs.Provider;

public class UpdateProviderRequest
{
    [Required]
    public int SpecialtyId { get; set; }

    [Required]
    public int ClinicId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? NPI { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }
}
