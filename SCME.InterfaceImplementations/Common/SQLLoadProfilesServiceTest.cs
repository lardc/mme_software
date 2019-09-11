using SCME.Types;
using SCME.Types.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCME.InterfaceImplementations.Common
{
    public class SQLLoadProfilesServiceTest : LoadProfilesService<SqlCommand, SqlConnection>
    {
        public SQLLoadProfilesServiceTest(SqlConnection dbConnection) : base(dbConnection) {  }

        public override void PrepareQueries()
        {
            _ChildsCmd = new SqlCommand(@"SELECT [PROF_ID], [PROF_NAME], [PROF_GUID], [PROF_VERS], [PROF_TS] FROM [dbo].[PROFILES] WHERE [PROF_NAME] = @PROF_NAME ORDER BY [PROF_TS] DESC", _Connection);
            _ChildsCmd.Parameters.Add("@PROF_NAME", SqlDbType.NVarChar, 32);
            _ChildsCmd.Prepare();

            _CondCmd = new SqlCommand(@"SELECT [PTT_ID], [TEST_TYPE_ID] FROM [dbo].[PROF_TEST_TYPE] WHERE [PROF_ID] = @PROF_ID", _Connection);
            _CondCmd.Parameters.Add("@PROF_ID", SqlDbType.Int);
            _CondCmd.Prepare();

            _ProfileSelect = new SqlCommand("SELECT P.[PROF_ID], P.[PROF_NAME], P.[PROF_GUID], P.[PROF_VERS] , PP.MAX_PROF_TS " +
                                              " FROM (SELECT [PROF_NAME], MAX([PROF_TS]) AS MAX_PROF_TS FROM [dbo].[PROFILES] WHERE (ISNULL([IS_DELETED], 0)=0) GROUP BY [PROF_NAME]) PP " +
                                              " INNER JOIN [dbo].[PROFILES] P ON PP.PROF_NAME = P.PROF_NAME AND PP.MAX_PROF_TS = P.PROF_TS", _Connection);

            _ProfileSelectWithMME = new SqlCommand(@"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, P.PROF_VERS, P.PROF_TS
                                 FROM (
                                        SELECT MAX(PR.PROF_ID) AS MAX_PROF_ID
                                        FROM PROFILES PR
                                        WHERE (ISNULL(PR.IS_DELETED, 0)=0)
                                        GROUP BY PR.PROF_NAME
	                                  ) PP
                                  INNER JOIN PROFILES P ON (P.PROF_ID=PP.MAX_PROF_ID)
	                              INNER JOIN MME_CODES_TO_PROFILES MCP ON (MCP.PROFILE_ID=P.PROF_ID)
                                  INNER JOIN MME_CODES MC ON (
                                                              (MC.MME_CODE_ID=MCP.MME_CODE_ID) AND
                                                              (MC.MME_CODE=@MME_CODE)
                                                             )", _Connection);
            _ProfileSelectWithMME.Parameters.Add("@MME_CODE", SqlDbType.NVarChar, 64);
            _ProfileSelectWithMME.Prepare();

            _OrderSelect = new SqlCommand(@"SELECT [ORD] FROM [dbo].[PROF_TEST_TYPE] WHERE [PTT_ID] = @TEST_TYPE_ID",_Connection);
            _OrderSelect.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _OrderSelect.Prepare();

            _CondSelect = new SqlCommand("SELECT C.[COND_NAME], PC.[VALUE] FROM [dbo].[PROF_COND] PC LEFT JOIN [dbo].[CONDITIONS] C on C.[COND_ID] = PC.[COND_ID] WHERE PC.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID",_Connection);
            _CondSelect.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _CondSelect.Prepare();

            _ParamSelect = new SqlCommand("SELECT P.[PARAM_NAME], PP.[MIN_VAL], PP.[MAX_VAL] FROM [dbo].[PROF_PARAM] PP LEFT JOIN [dbo].[PARAMS] P on P.[PARAM_ID] = PP.[PARAM_ID] WHERE PP.[PROF_TESTTYPE_ID] = @TEST_TYPE_ID",_Connection);
            _ParamSelect.Parameters.Add("@TEST_TYPE_ID", SqlDbType.Int);
            _ParamSelect.Prepare();

            //чтение одного профиля последней редакции по его PROF_NAME и MME_CODE
            _ProfileSingleSelect = new SqlCommand(@"SELECT P.PROF_ID, P.PROF_NAME, P.PROF_GUID, P.PROF_VERS, P.PROF_TS
                                                    FROM (
                                                           SELECT MAX(PR.PROF_ID) AS MAX_PROF_ID
                                                           FROM PROFILES PR
                                                           WHERE (
                                                                  (PR.PROF_NAME=@ProfName) AND
                                                                  (ISNULL(PR.IS_DELETED, 0)=0)
                                                                 )
                                                         ) PP
                                                     INNER JOIN PROFILES P ON (P.PROF_ID=PP.MAX_PROF_ID)
                                                     INNER JOIN MME_CODES_TO_PROFILES MCP ON (MCP.PROFILE_ID=P.PROF_ID)
                                                     INNER JOIN MME_CODES MC ON (
                                                                                 (MC.MME_CODE_ID=MCP.MME_CODE_ID) AND
                                                                                 (MC.MME_CODE=@MmmeCode)
                                                                                )", _Connection);
            _ProfileSingleSelect.Parameters.Add("@ProfName", SqlDbType.NVarChar, 32);
            _ProfileSingleSelect.Parameters.Add("@MmmeCode", SqlDbType.NVarChar, 64);
            _ProfileSingleSelect.Prepare();

            _ProfileByKey = new SqlCommand(@"SELECT PROF_ID, PROF_NAME, PROF_GUID, PROF_VERS, PROF_TS FROM PROFILES WHERE PROF_GUID = @PROF_GUID", _Connection);
            _ProfileByKey.Parameters.Add("@PROF_GUID", SqlDbType.UniqueIdentifier);
            _ProfileByKey.Prepare();

            _GetMMECodes = new SqlCommand(@"SELECT MME_CODE FROM MME_CODES", _Connection);
            _GetMMECodes.Prepare();
        }
    }
}
