using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot2Project.Domain.Ports;
using Iot2Project.Domain.Entities;
using Dapper;
using System.Data;

namespace Iot2Project.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }



        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            const string sql = @"
            INSERT INTO iot_project.users
                  (full_name, email, password_hash, user_profile_id,
                   is_deleted, created_at, updated_at)
            VALUES (@FullName, @Email, @PasswordHash, @UserProfileId,
                    FALSE, @CreatedAt, @UpdatedAt)
            RETURNING user_id  AS UserId,
                      full_name       AS FullName,
                      email,
                      user_profile_id AS UserProfileId,
                      is_deleted      AS IsDeleted,
                      created_at,
                      updated_at;";

            return await _connection.QuerySingleAsync<User>(new CommandDefinition(sql, user, cancellationToken: ct));

        }

        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    user_id AS UserId,
                    full_name AS FullName,
                    password_hash AS PasswordHash,
                    email AS Email,
                    user_profile_id AS UserProfileId,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    is_deleted AS IsDeleted  
                FROM iot_project.users
                WHERE is_deleted = false;";

            return await _connection.QueryAsync<User>(sql);
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    user_id AS UserId,
                    full_name AS FullName,
                    password_hash AS PasswordHash,
                    email AS Email,
                    user_profile_id AS UserProfileId,
                    created_at AS CreatedAt,
                    updated_at AS UpdatedAt,
                    is_deleted AS IsDeleted 
                FROM iot_project.users
                WHERE user_id = @id AND is_deleted = false
                LIMIT 1;";

            return await _connection.QueryFirstOrDefaultAsync<User>(sql, new { id });
        }

        public async Task<User?> UpdateAsync(int id, User user, CancellationToken ct = default)
        {
            const string sql = @"
                UPDATE iot_project.users
                SET full_name = @FullName,
                    email = @Email,
                    password_hash = @PasswordHash,
                    user_profile_id = @UserProfileId,
                    updated_at = @UpdatedAt
                WHERE user_id = @Id AND is_deleted = false
                RETURNING user_id AS UserId,
                          full_name AS FullName,
                          email AS Email,
                          user_profile_id AS UserProfileId,
                          is_deleted AS IsDeleted,
                          created_at AS CreatedAt,
                          updated_at AS UpdatedAt;";

            return await _connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Id = id, user.FullName, user.Email, user.PasswordHash, user.UserProfileId, user.UpdatedAt }, cancellationToken: ct));
        }


        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            const string sql = @"
                UPDATE iot_project.users
                SET is_deleted = true,
                    updated_at = @UpdatedAt
                WHERE user_id = @Id AND is_deleted = false;";

            var rowsAffected = await _connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, UpdatedAt = DateTime.UtcNow }, cancellationToken: ct));
            return rowsAffected > 0;
        }




    }
}
