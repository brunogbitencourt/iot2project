﻿namespace Iot2Project.Domain.Entities;

public class DeviceData
{   
    public int DeviceDataId { get; set; }
    public int DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public float Value { get; set; }
    public int UserId { get; set; }
    public string Command { get; set; }
}
