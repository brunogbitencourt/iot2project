using Iot2Project.Domain.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Devices.GetDevices
{
    public sealed class GetDeviceByidService
    {
        private readonly IDeviceRepository _repo;

        public GetDeviceByidService(IDeviceRepository repo) => _repo = repo;    

        public async Task<DeviceDto?> ExecuteAsync(int id, CancellationToken ct = default)
        {
            var device = await _repo.GetDeviceByIdAsync(id);
            return device is null ? null
                                  : new DeviceDto(
                                      device.DeviceId,
                                      device.UserId,
                                      device.IsDeleted,
                                      device.CreatedAt,
                                      device.UpdatedAt,
                                      device.ConnectedPort,
                                      device.Name,
                                      device.Type,
                                      device.Category,
                                      device.Unit,
                                      device.MqttTopic,
                                      device.KafkaTopic
                                  );
        }

    }
}
