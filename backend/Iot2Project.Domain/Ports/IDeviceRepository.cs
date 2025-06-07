using Iot2Project.Domain.Entities;

namespace Iot2Project.Domain.Ports;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetAllAsync();
    Task<IEnumerable<string>> GetMqttTopicsAsync();
    Task<Device?> GetByMqttTopicAsync(string topic);
}
