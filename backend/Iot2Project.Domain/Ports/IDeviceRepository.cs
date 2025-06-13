using Iot2Project.Domain.Entities;

namespace Iot2Project.Domain.Ports;

public interface IDeviceRepository
{
    Task<IEnumerable<Device>> GetAllAsync();
    Task<IEnumerable<string>> GetMqttTopicsAsync();
    Task<Device?> GetByMqttTopicAsync(string topic);

    Task<Device?> GetDeviceByIdAsync(int deviceId);

    Task<Device?> CreateDeviceAsync(Device device, CancellationToken ct = default);

    Task<Device?> UpdateDeviceAsync(int deviceId, Device device, CancellationToken ct = default);

    Task<bool> DeleteDeviceAsync(int deviceId, CancellationToken ct = default);


}
