using Iot2Project.Domain.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Devices.GetDevices
{
    public sealed class GetAllDevicesService
    {
        private readonly IDeviceRepository _repo;

        public GetAllDevicesService(IDeviceRepository repo) => _repo = repo;

        public async Task<IEnumerable<DeviceDto>> ExecuteAsync(CancellationToken ct = default)
        {
            var devices = await _repo.GetAllAsync();
     
            return devices.Select(u => new DeviceDto(
                u.DeviceId,
                u.UserId,
                u.IsDeleted, 
                u.CreatedAt, 
                u.UpdatedAt, 
                u.ConnectedPort, 
                u.Name,
                u.Type,
                u.Category,
                u.Unit,
                u.MqttTopic,
                u.KafkaTopic
                ));
        }


    }
}
