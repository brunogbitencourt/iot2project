using Iot2Project.Domain.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Devices.DeleteDevice
{
    public sealed class DeleteDeviceService
    {
        private readonly IDeviceRepository _repo;

        public DeleteDeviceService(IDeviceRepository repo) => _repo = repo;

        public async Task<bool> DeleteDeviceAsync(int deviceId, CancellationToken ct = default)
            => await _repo.DeleteDeviceAsync(deviceId, ct);

    }
}
