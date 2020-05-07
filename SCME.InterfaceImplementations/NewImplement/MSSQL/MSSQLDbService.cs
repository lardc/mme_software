using System.Data.SqlClient;
using System.ServiceModel;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MSSQLDbService : Common.DbService.DbService<SqlCommand, SqlConnection>  
    {
        public MSSQLDbService(SqlConnection connection, bool enabledCache = true) : base(connection, enabledCache)
        {
        }
    }
}