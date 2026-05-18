using System.Data.Common;

namespace Taller_Mecanico_Users.Infrastructure.Persistence
{
    public interface ISqlConnectionFactory
    {
        DbConnection CreateConnection();
    }
}

