using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;            // a interface IDeviceDataRepository

namespace Iot2Project.Infrastructure.Persistence.Repositories
{
    public class DeviceDataRepository : IDeviceDataRepository
    {
        private readonly IDbConnection _connection;

        public DeviceDataRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Persiste um novo ponto de telemetria.
        /// </summary>
        public async Task SaveAsync(DeviceData data, CancellationToken ct = default)
        {
            const string sql = @"
                INSERT INTO iot_project.device_data 
                    (device_id, timestamp, value, user_id, command)
                VALUES 
                    (@DeviceId, @Timestamp, @Value, @UserId, @Command);";

            await _connection.ExecuteAsync(
                new CommandDefinition(sql, data, cancellationToken: ct)
            );
        }

        /// <summary>
        /// Retorna todas as leituras de um dispositivo num intervalo (brutas, sem paginação).
        /// </summary>
        public async Task<IEnumerable<DeviceData>> GetByPeriodAsync(
            int deviceId,
            DateTime from,
            DateTime to,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    device_data_id AS DeviceDataId,
                    device_id      AS DeviceId,
                    timestamp,
                    value,
                    user_id        AS UserId,
                    command
                FROM iot_project.device_data
                WHERE device_id = @DeviceId
                  AND timestamp BETWEEN @From AND @To                  
                ORDER BY timestamp;";

            return await _connection.QueryAsync<DeviceData>(
                new CommandDefinition(sql,
                    new { DeviceId = deviceId, From = from, To = to },
                    cancellationToken: ct
                )
            );
        }

        /// <summary>
        /// Retorna a última leitura registrada para um dispositivo.
        /// </summary>
        public async Task<DeviceData?> GetLatestAsync(
            int deviceId,
            CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    device_data_id AS DeviceDataId,
                    device_id      AS DeviceId,
                    timestamp,
                    value,
                    user_id        AS UserId,
                    command
                FROM iot_project.device_data
                WHERE device_id = @DeviceId
                ORDER BY timestamp DESC
                LIMIT 1;";

            return await _connection.QueryFirstOrDefaultAsync<DeviceData>(
                new CommandDefinition(sql,
                    new { DeviceId = deviceId },
                    cancellationToken: ct
                )
            );
        }
    }
}
