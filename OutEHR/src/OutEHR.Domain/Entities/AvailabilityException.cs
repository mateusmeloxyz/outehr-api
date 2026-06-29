namespace OutEHR.Domain.Entities;

public class AvailabilityException{
    public int Id { get; set; }
    public int ProviderId { get; set; }
    public DateOnly ExceptionDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Reason { get; set; }
    public bool IsUnavailable { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
