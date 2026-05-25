using System.Data.Common;

namespace Taller_Mecanico_Services.Infrastructure.Persistence
{
    public interface ISqlConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
