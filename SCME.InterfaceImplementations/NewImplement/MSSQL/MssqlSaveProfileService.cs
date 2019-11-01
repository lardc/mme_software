using SCME.InterfaceImplementations.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.NewImplement.MSSQL
{
    public class MssqlSaveProfileService : SaveProfilesService<SqlCommand, SqlConnection>
    {
        public MssqlSaveProfileService(SqlConnection dbConnection) : base(dbConnection) { }
    }
}
