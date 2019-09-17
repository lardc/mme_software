using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SCME.InterfaceImplementations.Common
{
    public class InserterBaseTestParametersAndNormatives
    {
        public int Order { get; set; }
        private int _TestTypeId;
        private int _ProfileId;

        private DbTransaction _DbTransaction;

        private DbCommand _ProfileTestTypeInsert;
        private DbCommand _ProfileParameterInsert;
        private DbCommand _ProfileConditionInsert;

        private Dictionary<string, int> _TestTypeIdByName;
        private Dictionary<string, int> _ConditionIdByName;
        private Dictionary<string, int> _ParameterIdByName;

        public InserterBaseTestParametersAndNormatives(int profileId, DbTransaction dbTransaction, DbCommand profileTestTypeInsert, DbCommand profileParameterInsert, DbCommand profileConditionInsert, Dictionary<string, int> testTypeIdByName, Dictionary<string, int> conditionIdByName, Dictionary<string, int> parameterIdByName)
        {
            _ProfileId = profileId;
            _DbTransaction = dbTransaction ?? throw new ArgumentNullException(nameof(dbTransaction));
            _ProfileTestTypeInsert = profileTestTypeInsert ?? throw new ArgumentNullException(nameof(profileTestTypeInsert));
            _ProfileParameterInsert = profileParameterInsert ?? throw new ArgumentNullException(nameof(profileParameterInsert));
            _ProfileConditionInsert = profileConditionInsert ?? throw new ArgumentNullException(nameof(profileConditionInsert));
            _TestTypeIdByName = testTypeIdByName ?? throw new ArgumentNullException(nameof(testTypeIdByName));
            _ConditionIdByName = conditionIdByName ?? throw new ArgumentNullException(nameof(conditionIdByName));
            _ParameterIdByName = parameterIdByName ?? throw new ArgumentNullException(nameof(parameterIdByName));
        }


        public void Insert(string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters)
        {
            Order++;
            _TestTypeId = InsertTestType(_TestTypeIdByName[typeName], _ProfileId);

            foreach (var i in conditions)
                InsertCondition(i.Key, i.Value);

            foreach (var i in parameters)
                InsertParameter(i.Key, i.Value.Min, i.Value.Max);
        }

        public void Insert((string typeName, Dictionary<string, object> conditions, Dictionary<string, (object Min, object Max)> parameters) data)
        {
            Insert(data.typeName, data.conditions, data.parameters);
        }

       

        private int InsertTestType(int typeId, int order = 0)
        {
            _ProfileTestTypeInsert.Parameters["@PROF_ID"].Value = _ProfileId;
            _ProfileTestTypeInsert.Parameters["@TEST_TYPE_ID"].Value = typeId;
            _ProfileTestTypeInsert.Parameters["@ORD"].Value = Order;
            _ProfileTestTypeInsert.Transaction = _DbTransaction;

            return Convert.ToInt32(_ProfileTestTypeInsert.ExecuteScalar());
        }

        protected void InsertParameter(string name, object min, object max)
        {
            _ProfileParameterInsert.Parameters["@PROF_TESTTYPE_ID"].Value = _TestTypeId;
            _ProfileParameterInsert.Parameters["@PROF_ID"].Value = _ProfileId;
            _ProfileParameterInsert.Parameters["@PARAM_ID"].Value = _ParameterIdByName[name];
            _ProfileParameterInsert.Parameters["@MIN_VAL"].Value = min;
            _ProfileParameterInsert.Parameters["@MAX_VAL"].Value = max;
            _ProfileParameterInsert.Transaction = _DbTransaction;

            _ProfileParameterInsert.ExecuteNonQuery();
        }

        protected void InsertCondition(string name, object value)
        {
            _ProfileConditionInsert.Parameters["@PROF_TESTTYPE_ID"].Value = _TestTypeId;
            _ProfileConditionInsert.Parameters["@PROF_ID"].Value = _ProfileId;
            _ProfileConditionInsert.Parameters["@COND_ID"].Value = _ConditionIdByName[name];
            _ProfileConditionInsert.Parameters["@VALUE"].Value = value.ToString();
            _ProfileConditionInsert.Transaction = _DbTransaction;

            _ProfileConditionInsert.ExecuteNonQuery();
        }
    }
}
