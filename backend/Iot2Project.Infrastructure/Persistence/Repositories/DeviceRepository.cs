using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;

namespace Iot2Project.Infrastructure.Persistence.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly IDbConnection _db;

    public DeviceRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Device>> GetAllAsync()
    {
        const string sql = @"
        SELECT
            device_id    AS DeviceId,
            user_id      AS UserId,
            is_deleted   AS IsDeleted,
            created_at   AS CreatedAt,
            updated_at   AS UpdatedAt,
            connected_port AS ConnectedPort,
            name         AS Name,
            type         AS Type,
            category     AS Category,
            unit         AS Unit,
            mqtt_topic   AS MqttTopic,
            kafka_topic  AS KafkaTopic
        FROM iot_project.device
        WHERE is_deleted = false;
    ";

        return await _db.QueryAsync<Device>(sql);
    }


    public async Task<IEnumerable<string>> GetMqttTopicsAsync()
    {
        var sql = "SELECT mqtt_topic FROM iot_project.device WHERE is_deleted = false AND mqtt_topic IS NOT NULL";
        return await _db.QueryAsync<string>(sql);
    }

    public async Task<Device?> GetByMqttTopicAsync(string topic)
    {
        const string sql = @"
        SELECT
            device_id    AS DeviceId,
            mqtt_topic   AS MqttTopic,
            kafka_topic  AS KafkaTopic,
            user_id      AS UserId
        FROM iot_project.device
        WHERE mqtt_topic = @topic AND is_deleted = false
        LIMIT 1;
    ";
        return await _db.QueryFirstOrDefaultAsync<Device>(sql, new { topic });
    }

}
