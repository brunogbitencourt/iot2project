namespace Iot2Project.Domain.Entities;

public class Device
{
    public int DeviceId { get; set; }
    public int UserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ConnectedPort { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public string? MqttTopic { get; set; }
    public string? KafkaTopic { get; set; }
}
