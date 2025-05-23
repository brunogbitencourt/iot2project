using System.Data;
using Dapper;
using Iot2Project.Application.Interfaces;
using Iot2Project.Domain.Entities;

namespace Iot2Project.Infrastructure.Persistence.Repositories;

public class DeviceDataRepository : IDeviceDataRepository
{
    private readonly IDbConnection _connection;

    public DeviceDataRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task SaveAsync(DeviceData data)
    {
       

        const string sql = @"
            INSERT INTO iot_project.device_data (
                device_id, timestamp, value, user_id, command
            ) VALUES (
                @DeviceId, @Timestamp, @Value, @UserId, @Command
            );";

        await _connection.ExecuteAsync(sql, data);
    }
}
