using Iot2Project.Domain.Entities;

namespace Iot2Project.Application.Interfaces;

public interface IDeviceDataRepository
{
    Task SaveAsync(DeviceData data);
}
