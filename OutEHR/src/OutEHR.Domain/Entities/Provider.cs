namespace OutEHR.Domain.Entities;

public class Provider{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int SpecialtyId { get; set; }
    public int ClinicId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? NPI { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public decimal Rating { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
