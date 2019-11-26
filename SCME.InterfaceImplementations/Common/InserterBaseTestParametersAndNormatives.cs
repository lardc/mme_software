using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SCME.InterfaceImplementations.Common
{
    public class InserterBaseTestParametersAndNormatives
    {
        public int Order { get; set; }
        private int _testTypeId;
        private readonly int _profileId;

        private readonly DbTransaction _dbTransaction;

        private readonly DbCommand _profileTestTypeInsert;
        private readonly DbCommand _profileParameterInsert;
        private readonly DbCommand _profileConditionInsert;

        private readonly Dictionary<string, int> _testTypeIdByName;
        private readonly Dictionary<string, int> _conditionIdByName;
        private readonly Dictionary<string, int> _parameterIdByName;

        private readonly Func<DbCommand, int> _lastInsertRowId;

        public InserterBaseTestParametersAndNormatives(int profileId, DbTransaction dbTransaction, DbCommand profileTestTypeInsert, DbCommand profileParameterInsert, DbCommand profileConditionInsert, Dictionary<string, int> testTypeIdByName,
            Dictionary<string, int> conditionIdByName, Dictionary<string, int> parameterIdByName, Func<DbCommand, int> lastInsertRowId)
        {
            _profileId = profileId;
            _dbTransaction = dbTransaction ?? throw new ArgumentNullException(nameof(dbTransaction));
            _profileTestTypeInsert = profileTestTypeInsert ?? throw new ArgumentNullException(nameof(profileTestTypeInsert));
            _profileParameterInsert = profileParameterInsert ?? throw new ArgumentNullException(nameof(profileParameterInsert));
            _profileConditionInsert = profileConditionInsert ?? throw new ArgumentNullException(nameof(profileConditionInsert));
            _testTypeIdByName = testTypeIdByName ?? throw new ArgumentNullException(nameof(testTypeIdByName));
            _conditionIdByName = conditionIdByName ?? throw new ArgumentNullException(nameof(conditionIdByName));
            _parameterIdByName = parameterIdByName ?? throw new ArgumentNullException(nameof(parameterIdByName));
            _lastInsertRowId = lastInsertRowId;
        }


        public void Insert(string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters)
        {
            Order++;
            _testTypeId = InsertTestType(_testTypeIdByName[typeName]);

            foreach (var i in conditions)
                InsertCondition(i.Key, i.Value);

            foreach (var i in parameters)
                InsertParameter(i.Key, i.Value.Min, i.Value.Max);
        }

        public void Insert((string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) data)
        {
            var (typeName, conditions, parameters) = data;
            Insert(typeName, conditions, parameters);
        }


        private int InsertTestType(int typeId)
        {
            _profileTestTypeInsert.Parameters["@PROF_ID"].Value = _profileId;
            _profileTestTypeInsert.Parameters["@TEST_TYPE_ID"].Value = typeId;
            _profileTestTypeInsert.Parameters["@ORD"].Value = Order;
            _profileTestTypeInsert.Transaction = _dbTransaction;

            return _lastInsertRowId(_profileTestTypeInsert);
            //return Convert.ToInt32(_ProfileTestTypeInsert.ExecuteScalar());
        }

        private void InsertParameter(string name, object min, object max)
        {
            _profileParameterInsert.Parameters["@PROF_TESTTYPE_ID"].Value = _testTypeId;
            _profileParameterInsert.Parameters["@PROF_ID"].Value = _profileId;
            _profileParameterInsert.Parameters["@PARAM_ID"].Value = _parameterIdByName[name];
            _profileParameterInsert.Parameters["@MIN_VAL"].Value = min;
            _profileParameterInsert.Parameters["@MAX_VAL"].Value = max;
            _profileParameterInsert.Transaction = _dbTransaction;

            _profileParameterInsert.ExecuteNonQuery();
        }

        private void InsertCondition(string name, object value)
        {
            _profileConditionInsert.Parameters["@PROF_TESTTYPE_ID"].Value = _testTypeId;
            _profileConditionInsert.Parameters["@PROF_ID"].Value = _profileId;
            try
            {
                _profileConditionInsert.Parameters["@COND_ID"].Value = _conditionIdByName[name];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            _profileConditionInsert.Parameters["@VALUE"].Value = value.ToString();
            _profileConditionInsert.Transaction = _dbTransaction;

            _profileConditionInsert.ExecuteNonQuery();
        }
    }
}