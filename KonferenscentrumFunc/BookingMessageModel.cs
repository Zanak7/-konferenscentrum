namespace KonferenscentrumFunc;

public class BookingMessageModel
{
    public int BookingId { get; set; }
    public string Action { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? Reason { get; set; }
    
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? At { get; set; }
}