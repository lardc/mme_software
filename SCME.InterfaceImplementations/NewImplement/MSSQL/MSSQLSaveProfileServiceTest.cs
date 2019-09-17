using SCME.InterfaceImplementations.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MSSQLSaveProfileServiceTest : SaveProfilesServiceTest<SqlCommand, SqlConnection>
    {
        public MSSQLSaveProfileServiceTest(SqlConnection dbConnection) : base(dbConnection) { }
    }
}
