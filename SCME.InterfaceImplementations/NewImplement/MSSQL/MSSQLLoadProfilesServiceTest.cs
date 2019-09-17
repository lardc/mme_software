using SCME.InterfaceImplementations.Common;
using System.Data.SqlClient;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MSSQLLoadProfilesServiceTest : LoadProfilesServiceTest<SqlCommand, SqlConnection>
    {
        public MSSQLLoadProfilesServiceTest(SqlConnection dbConnection) : base(dbConnection) {  }
    }
}
