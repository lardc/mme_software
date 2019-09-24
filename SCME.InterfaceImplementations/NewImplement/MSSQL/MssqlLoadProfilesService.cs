using SCME.InterfaceImplementations.Common;
using System.Data.SqlClient;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MssqlLoadProfilesService : LoadProfilesService<SqlCommand, SqlConnection>
    {
        public MssqlLoadProfilesService(SqlConnection dbConnection) : base(dbConnection) {  }
    }
}
