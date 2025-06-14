﻿using Iot2Project.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Iot2Project.Domain.Ports;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, DeviceData data, CancellationToken ct = default);
}
