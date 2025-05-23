using System.Data;

namespace Iot2Project.Infrastructure.Persistence.Context;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
