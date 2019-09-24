using System.Data.SqlClient;
using SCME.InterfaceImplementations.Common;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MSSQLDbService : Common.DbService.DbService<SqlCommand, SqlConnection>  
    {
        public MSSQLDbService(SqlConnection connection) : base(connection)
        {
        }
    }
}