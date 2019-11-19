using System.Data.SqlClient;
using System.ServiceModel;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single,
        Namespace = "http://proton-electrotex.com/SCME")]
    public class MSSQLDbService : Common.DbService.DbService<SqlCommand, SqlConnection>  
    {
        public MSSQLDbService(SqlConnection connection) : base(connection)
        {
        }
    }
}