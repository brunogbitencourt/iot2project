namespace Iot2Project.Domain.Entities;

public class DeviceData
{
    public int DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? Value { get; set; }
    public int? UserId { get; set; }
    public string? Command { get; set; }
}
