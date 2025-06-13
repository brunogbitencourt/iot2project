using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
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

    public async Task<Device?> GetDeviceByIdAsync(int deviceID)
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
        WHERE is_deleted = false
        AND device_id = @deviceID;
        ";

        return await _db.QueryFirstOrDefaultAsync<Device>(sql, new { deviceID });
    }

    public async Task<Device?> CreateDeviceAsync(Device device, CancellationToken ct = default)
    {
        const string sql = @"
        INSERT INTO iot_project.device (
            user_id,
            connected_port,
            name,
            type,
            category,
            unit,
            mqtt_topic,
            kafka_topic
        ) VALUES (
            @UserId,
            @ConnectedPort,
            @Name,
            @Type,
            @Category,
            @Unit,
            @MqttTopic,
            @KafkaTopic
        )
        RETURNING
            device_id AS DeviceId,
            user_id AS UserId,
            is_deleted AS IsDeleted,
            created_at AS CreatedAt,
            updated_at AS UpdatedAt;";

        return await _db.QueryFirstOrDefaultAsync<Device>(sql, device);
    }

    public async Task<Device?> UpdateDeviceAsync(int deviceID, Device device, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE iot_project.device
            SET
                user_id        = @UserId,
                connected_port = @ConnectedPort,
                name           = @Name,
                type           = @Type,
                category       = @Category,
                unit           = @Unit,
                mqtt_topic     = @MqttTopic,
                kafka_topic    = @KafkaTopic,
                updated_at     = NOW()
                WHERE device_id   = @DeviceID
                RETURNING
                device_id       AS DeviceId,
                user_id         AS UserId,
                is_deleted      AS IsDeleted,
                created_at      AS CreatedAt,
                updated_at      AS UpdatedAt,
                connected_port  AS ConnectedPort,
                name            AS Name,
                type            AS Type,
                category        AS Category,
                unit            AS Unit,
                mqtt_topic      AS MqttTopic,
                kafka_topic     AS KafkaTopic;
            ";

        var parameters = new
        {
            DeviceID = deviceID,
            UserId = device.UserId,
            ConnectedPort = device.ConnectedPort,
            Name = device.Name,
            Type = device.Type,
            Category = device.Category,
            Unit = device.Unit,
            MqttTopic = device.MqttTopic,
            KafkaTopic = device.KafkaTopic
        };

        return await _db.QueryFirstOrDefaultAsync<Device>(sql, parameters);
    }


    public async Task<bool> DeleteDeviceAsync(int deviceID, CancellationToken ct=default)
    {
        const string sql = @"
            UPDATE iot_project.device  
             SET is_deleted = true,
                    updated_at = @UpdatedAt
                WHERE device_id  = @Id AND is_deleted = false;        
        ";

        var rowsAfdected = await _db.ExecuteAsync(new CommandDefinition(sql, new { Id = deviceID, UpdatedAt = DateTime.UtcNow }, cancellationToken: ct));

        return rowsAfdected > 0;
    }



}
