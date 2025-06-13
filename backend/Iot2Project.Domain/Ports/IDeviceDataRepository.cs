// Domain/Ports/IDeviceDataRepository.cs
using Iot2Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Domain.Ports
{
    public interface IDeviceDataRepository
    {
        Task SaveAsync(DeviceData data, CancellationToken ct = default);

        // Só devolve a lista bruta de leituras — sem paginação
        Task<IEnumerable<DeviceData>> GetByPeriodAsync(
            int deviceId,
            DateTime from,
            DateTime to,
            CancellationToken ct = default
        );

        // Última leitura
        Task<DeviceData?> GetLatestAsync(int deviceId, CancellationToken ct = default);
    }
}
