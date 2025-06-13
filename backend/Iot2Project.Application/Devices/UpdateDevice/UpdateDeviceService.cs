using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot2Project.Application.Devices.GetDevices;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;

namespace Iot2Project.Application.Devices.UpdateDevice
{
    public sealed class UpdateDeviceService
    {
        private readonly IDeviceRepository _repo;

        public UpdateDeviceService(IDeviceRepository repo) => _repo = repo;

        public async Task<Device> ExecuteAsync(int deviceId, UpdateDeviceDTO request, CancellationToken ct = default)
        {
            var device = await _repo.GetDeviceByIdAsync(deviceId);
            if (device == null)
            {
                return null; // Device not found
            }

            Console.WriteLine(device.Name, device.DeviceId);

            if (!string.IsNullOrWhiteSpace(request.Name))
                device.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Unit))
                device.Unit = request.Unit;

            if (!string.IsNullOrWhiteSpace(request.Type))
                device.Type = request.Type;

            if (!string.IsNullOrWhiteSpace(request.Category))
                device.Category = request.Category;

            if (!string.IsNullOrWhiteSpace(request.ConnectedPort))
                device.ConnectedPort = request.ConnectedPort;

            if (!string.IsNullOrWhiteSpace(request.KafkaTopic))
                device.KafkaTopic = request.KafkaTopic;

            if(!string.IsNullOrWhiteSpace(request.MqttTopic))
                device.MqttTopic = request.MqttTopic;

             device.UserId = request.UserId;

            var deviceUpdated = await _repo.UpdateDeviceAsync(deviceId, device, ct);
            return deviceUpdated; // Update successful
        }
    }
}
