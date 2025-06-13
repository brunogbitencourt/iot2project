using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;

namespace Iot2Project.Application.Devices.CreateDevice
{
    public sealed class CreateDeviceService
    {
        private readonly IDeviceRepository _repo;

        public CreateDeviceService(IDeviceRepository repo) => _repo = repo;

        public async Task<Device?> ExecuteAsync(CreateDeviceRequest request, CancellationToken ct = default)
        {
            var device = new Domain.Entities.Device
            {
                Name = request.Name,
                Type = request.Type,
                Category = request.Category,
                Unit = request.Unit,
                ConnectedPort = request.ConnectedPort,
                KafkaTopic = request.KafkaTopic,
                MqttTopic = request.MqttTopic,
                UserId = request.UserId
            };

            await _repo.CreateDeviceAsync(device, ct);
            return device;
        }

    }
}
