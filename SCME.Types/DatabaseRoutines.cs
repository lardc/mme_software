using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using SCME.Types.Profiles;

namespace SCME.Types
{   
    public static class DBConnections
    {
        private static SqlConnection FConnection = null;
        private static SqlConnection FConnectionDC = null;

        public static SqlConnection Connection
        {
            //установка соединения с базой данных SCME.dbViewer
            get
            {
                if (FConnection == null)
                    FConnection = new SqlConnection("server=192.168.0.134, 1444; uid=sa; pwd=Hpl1520; database=SCME_ResultsDB");

                return FConnection;
            }
        }

        public static SqlConnection ConnectionDC
        {
            //установка соединения с базой данных DC
            get
            {
                if (FConnectionDC == null)
                    FConnectionDC = new SqlConnection("server=sa-011; uid=sa; pwd=Hpl1520; database=SL_PE_DC20002");

                return FConnectionDC;
            }
        }
    }

    static public class DbRoutines
    {
        const string cReadedRecordNotSingle = "Readed record not single({ 0 }). One record was expected. { 1}.";

        #region Permissions

        public static bool UserPermissions(long userID, out ulong permissionsLo)
        {
            //чтение битовой маски, хранящей права пользователя в данном приложении
            //возвращает:
            //true  - пользователь userID является пользователем приложения;
            //false - пользователь userID не является пользователем приложения (при этом он может быть пользователем системы DC)
            int count = 0;
            ulong readedPermissionsLo = 0;

            SqlConnection connection = DBConnections.Connection;

            connection.Open();

            try
            {
                string sql = "SELECT PermissionsLo" +
                             " FROM Users" +
                             string.Format(" WHERE (UserID='{0}')", userID);

                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        readedPermissionsLo = Convert.ToUInt64(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            switch (count)
            {
                case 0:
                    //пользователь userID не является пользователем приложения
                    permissionsLo = 0;
                    return false;

                case 1:
                    //пользователь userID является пользователем приложения
                    permissionsLo = readedPermissionsLo;
                    return true;

                default:
                    //считано более одной записи для userID
                    throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), userID.ToString()));
            }
        }

        #endregion

        #region Users

        public static bool UserLoginByUserID(long userID, out string userLogin)
        {
            //чтение имени пользователя UserLogin из системы DC по принятому идентификатору пользователя userID
            //возвращает:
            //true  - пользователь userID зарегистрирован в системе DC;
            //false - пользователь userID не является пользователем приложения (при этом он может быть пользователем системы DC)
            string readedUserLogin = null;
            int count = 0;

            SqlConnection connection = DBConnections.ConnectionDC;

            connection.Open();

            try
            {
                string sql = "SELECT USERLOGIN" +
                             " FROM RUSDC_Users" +
                             string.Format(" WHERE (ID='{0}')", userID);

                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        readedUserLogin = Convert.ToString(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            switch (count)
            {
                case 0:
                    //пользователь userID не имеет регистрации в системе DC
                    userLogin = null;
                    return false;

                case 1:
                    //пользователь userID имеет регистрацию в системе DC
                    userLogin = readedUserLogin;
                    return true;

                default:
                    //считано более одной записи для userID
                    throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), "userID=" + userID.ToString()));
            }

        }

        public static bool FullUserNameByUserID(long userID, out string fullUserName)
        {
            //чтение полного имени пользователя из системы DC по принятому идентификатору пользователя userID
            //возвращает:
            //true  - пользователь userID зарегистрирован в системе DC;
            //false - пользователь userID не является пользователем приложения (при этом он может быть пользователем системы DC)
            string readedFullUserName = null;
            int count = 0;

            SqlConnection connection = DBConnections.ConnectionDC;

            connection.Open();

            try
            {
                string sql = "SELECT CONCAT(LASTNAME, ' ', CONCAT(SUBSTRING(FIRSTNAME, 1, 1), '.', SUBSTRING(MIDDLENAME, 1, 1), '.'))" +
                             " FROM RUSDC_USERS" +
                             string.Format(" WHERE (ID='{0}')", userID);

                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        readedFullUserName = Convert.ToString(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            switch (count)
            {
                case 0:
                    //пользователь userID не имеет регистрации в системе DC
                    fullUserName = null;
                    return false;

                case 1:
                    //пользователь userID имеет регистрацию в системе DC
                    fullUserName = readedFullUserName;
                    return true;

                default:
                    //считано более одной записи для userID
                    throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), "userID=" + userID.ToString()));
            }
        }

        public static long CheckDCUserExist(string userName, string userPassword)
        {
            //проверяет наличие записи в талице RUSDC_USERS для сочетания User_Name - UserPassword
            //возвращает:
            // -1 - введённый пароль неверен, либо пользователя с именем userName не существует;
            // -2 - введённый пароль неверен;
            // больше, чем ноль - идентификатор пользователя, пользователь userName является пользователем DC
            string sql = "SELECT ID, USERPASSWORD" +
                         " FROM RUSDC_USERS WITH(NOLOCK)" +
                         " WHERE (" +
                         string.Format("USERLOGIN='{0}' AND ", userName) +
                         string.Format("USERPASSWORD='{0}'", userPassword) +
                         "       )";

            long userID = -1;
            string password = null;
            int count = 0;

            SqlConnection connection = DBConnections.ConnectionDC;
            connection.Open();

            try
            {
                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        userID = Convert.ToInt64(values[0]);
                        password = (values[1] == DBNull.Value) ? null : values[1].ToString();

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }

                switch (count)
                {
                    case 0:
                        //введённый пароль неверен, либо пользователя с именем userName не существует
                        return -1;

                    case 1:
                        //запись найдена, но её поиск выполнялся по значению поля 'userPassword' без учёта регистра написания, проверяем введённый пароль с учётом регистра
                        return (userPassword == password) ? userID : -2;

                    default:
                        //считано более одной записи для userName
                        throw new Exception(string.Format("Считано более одной записи ({0}) из DC для пользователя '{1}'. Ожидалось либо ноль, либо одна запись.", count.ToString(), userName));
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void InsertToUsers(long userID, ulong permissionsLo)
        {
            //вставка новой записи в таблицу USERS
            SqlConnection connection = DBConnections.Connection;

            string sql = "INSERT INTO USERS(USERID, PERMISSIONSLO)" +
                         " VALUES (@UserID, @PermissionsLo)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userID;
            command.Parameters.Add("@PermissionsLo", SqlDbType.BigInt).Value = permissionsLo;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void UpdateUsers(long userID, ulong permissionsLo)
        {
            //изменение битовой маски прав permissionsLo пользователя userID в таблице USERS
            SqlConnection connection = DBConnections.Connection;

            string sql = "UPDATE USERS" +
                         " SET PERMISSIONSLO=@PermissionsLo" +
                         " WHERE (USERID=@UserID)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userID;
            command.Parameters.Add("@PermissionsLo", SqlDbType.BigInt).Value = permissionsLo;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        public static void DeleteFromUsers(long userID)
        {
            //удаление записи о пользователе userID из таблицы USERS
            SqlConnection connection = DBConnections.Connection;

            string sql = "DELETE" +
                         " FROM USERS" +
                         " WHERE (USERID=@UserID)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userID;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        public static void SaveToUsers(long userID, ulong permissionsLo)
        {
            //выполняет сохранение битовой маски разрешений permissionsLo для пользователя userID
            //проверяем наличие пользователя userID в таблице Users
            ulong oldPermissionsLo;
            if (UserPermissions(userID, out oldPermissionsLo))
            {
                //пользователь userID имеет описание своих прав в Users
                if (permissionsLo != oldPermissionsLo)
                {
                    //сохранённая битовая маска разрешений отличается от сохраняемой
                    UpdateUsers(userID, permissionsLo);
                }
            }
            else
            {
                //пользователь userID не имеет описания своих прав в Users
                InsertToUsers(userID, permissionsLo);
            }
        }

        #endregion

        #region ManualInputParams

        private static bool CheckManualInputParamExist(string name, TemperatureCondition temperatureCondition, out int manualInputParamID)
        {
            //проверяет наличие записи в талице MANUALINPUTPARAMS
            //возвращает:
            // true - в таблице ManualInputParams обнаружена единственная запись с принятыми name и temperatureCondition. в manualInputParamID будет возвращён идентификатор найденного параметра;
            // false - в таблице ManualInputParams нет ни одной записи с принятым name;
            string sql = "SELECT MANUALINPUTPARAMID" +
                         " FROM MANUALINPUTPARAMS WITH(NOLOCK)" +
                         " WHERE (" +
                         "        (NAME=@Name) AND" +
                         "        (TEMPERATURECONDITION=@TemperatureCondition)" +
                         "       )";

            manualInputParamID = -1;
            int count = 0;

            SqlConnection connection = DBConnections.Connection;
            connection.Open();

            try
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
                command.Parameters.Add("@TemperatureCondition", SqlDbType.NVarChar).Value = temperatureCondition.ToString();

                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        manualInputParamID = Convert.ToInt32(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            switch (count)
            {
                case 0:
                    //параметра с принятыми name и temperatureCondition не существует в таблице ManualInputParams
                    return false;

                case 1:
                    //запись найдена
                    return true;

                default:
                    //считано более одной записи для принятых name и temperatureCondition
                    throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), string.Concat("name=", name, "temperatureCondition=", temperatureCondition.ToString())));
            }
        }

        public static bool CheckManualInputParamExist(string temperatureConditionAndName, out int manualInputParamID)
        {
            //проверяет наличие записи в талице MANUALINPUTPARAMS
            //во входном параметре temperatureConditionAndName принимает строку слепленную из temperatureCondition и Name
            //возвращает:
            // true - в таблице ManualInputParams обнаружена единственная запись с принятыми name и temperatureCondition. в manualInputParamID будет возвращён идентификатор найденного параметра;
            // false - в таблице ManualInputParams нет ни одной записи с принятым name;
            string sql = "SELECT MANUALINPUTPARAMID" +
                         " FROM MANUALINPUTPARAMS WITH(NOLOCK)" +
                         " WHERE (" +
                         "        CONCAT(TEMPERATURECONDITION, NAME)=@TemperatureConditionAndName" +
                         "       )";

            manualInputParamID = -1;
            int count = 0;

            SqlConnection connection = DBConnections.Connection;
            connection.Open();

            try
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add("@TemperatureConditionAndName", SqlDbType.NVarChar).Value = temperatureConditionAndName;

                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        manualInputParamID = Convert.ToInt32(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            switch (count)
            {
                case 0:
                    //искомого параметра не существует в таблице ManualInputParams
                    return false;

                case 1:
                    //запись найдена
                    return true;

                default:
                    //считано более одной записи для принятых name и temperatureCondition
                    throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), temperatureConditionAndName));
            }
        }

        private static int InsertToManualInputParam(string name, TemperatureCondition temperatureCondition, string um, string descrEN, string descrRU)
        {
            //вставка новой записи в таблицу MANUALINPUTPARAMS
            int manualInputParamID = -1;

            SqlConnection connection = DBConnections.Connection;

            string sql = "INSERT INTO MANUALINPUTPARAMS(NAME, TEMPERATURECONDITION, UM, DESCREN, DESCRRU)" +
                         " OUTPUT INSERTED.MANUALINPUTPARAMID VALUES (@Name, @TemperatureCondition, @Um, @DescrEN, @DescrRU)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
            command.Parameters.Add("@TemperatureCondition", SqlDbType.NVarChar).Value = temperatureCondition.ToString();
            command.Parameters.Add("@Um", SqlDbType.NVarChar).Value = um;
            command.Parameters.Add("@DescrEN", SqlDbType.NVarChar).Value = descrEN;
            command.Parameters.Add("@DescrRU", SqlDbType.NVarChar).Value = descrRU;

            connection.Open();

            try
            {
                //считываем идентификатор созданного параметра
                manualInputParamID = (int)command.ExecuteScalar();
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return manualInputParamID;
        }

        private static void UpdateManualInputParam(int manualInputParamID, string name, TemperatureCondition temperatureCondition, string um, string descrEN, string descrRU)
        {
            //изменение реквизитов параметра в таблице MANUALINPUTPARAMS
            SqlConnection connection = DBConnections.Connection;

            string sql = "UPDATE MANUALINPUTPARAMS" +
                         " SET NAME=@Name," +
                         "     TEMPERATURECONDITION=@TemperatureCondition," +
                         "     UM=@Um," +
                         "     DESCREN=@DescrEN," +
                         "     DESCRRU=@DescrRU" +
                         " WHERE (MANUALINPUTPARAMID=@ManualInputParamID)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
            command.Parameters.Add("@TemperatureCondition", SqlDbType.NVarChar).Value = temperatureCondition.ToString();
            command.Parameters.Add("@Um", SqlDbType.NVarChar).Value = um;
            command.Parameters.Add("@DescrEN", SqlDbType.NVarChar).Value = descrEN;
            command.Parameters.Add("@DescrRU", SqlDbType.NVarChar).Value = descrRU;
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;
            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void DeleteFromManualInputParams(int manualInputParamID)
        {
            //удаление записи о вручную созданном параметре из таблицы MANUALINPUTPARAMS с идентификатором manualInputParamID
            SqlConnection connection = DBConnections.Connection;

            string sql = "DELETE" +
                         " FROM MANUALINPUTPARAMS" +
                         " WHERE (MANUALINPUTPARAMID=@ManualInputParamID)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        public static void DeleteAllByManualInputParamID(int manualInputParamID)
        {
            //сначала удаляет все определения параметра с принятым manualInputParamID в таблице ManualInputDevParam, после чего удаляет данный параметр из справочника MANUALINPUTDEVPARAM
            DeleteFromManualInputDevParam(manualInputParamID);
            DeleteFromManualInputParams(manualInputParamID);
        }

        public static void SaveToManualInputParams(int? manualInputParamID, string name, TemperatureCondition temperatureCondition, string um, string descrEN, string descrRU)
        {
            //выполняет сохранение параметра не предназначенного для измерения КИП СПП (его значение может быть введено пользователем вручную, оно никогда не будет измеряться средствами КИП СПП)
            //если принятый manualInputParamID=null - выполняется попытка создания нового параметра, иначе выполняется редактирование параметра
            if (manualInputParamID == null)
            {
                //выполняется попытка создания параметра
                InsertToManualInputParam(name, temperatureCondition, um, descrEN, descrRU);
            }
            else
            {
                //выполняется попытка редактирования параметра
                UpdateManualInputParam((int)manualInputParamID, name, temperatureCondition, um, descrEN, descrRU);
            }
        }

        #endregion

        #region ManualInputDevParam

        public static void InsertToManualInputDevParam(int dev_ID, int manualInputParamID, double value)
        {
            //вставка новой записи в таблицу ManualInputDevParam
            SqlConnection connection = DBConnections.Connection;

            string sql = "INSERT INTO MANUALINPUTDEVPARAM(DEV_ID, MANUALINPUTPARAMID, VALUE)" +
                         " VALUES (@Dev_ID, @ManualInputParamID, @Value)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = dev_ID;
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;
            command.Parameters.Add("@Value", SqlDbType.Decimal).Value = value;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void UpdateManualInputDevParam(int dev_ID, int manualInputParamID, double value)
        {
            //редактирование записи в таблице MANUALINPUTDEVPARAM
            SqlConnection connection = DBConnections.Connection;

            string sql = "UPDATE MANUALINPUTDEVPARAM" +
                         " SET VALUE=@Value" +
                         " WHERE (" +
                         "        (DEV_ID=@Dev_ID) AND" +
                         "        (MANUALINPUTPARAMID=@ManualInputParamID)" +
                         "       )";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = dev_ID;
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;
            command.Parameters.Add("@Value", SqlDbType.Decimal).Value = value;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static bool ExistManualInputDevParam(int dev_ID, int manualInputParamID)
        {
            //проверяет наличие записи в талице MANUALINPUTDEVPARAM для сочетания dev_ID - manualInputParamID
            //возвращает:
            // true - сочетание dev_ID - manualInputParamID найдено;
            // false - сочетания dev_ID - manualInputParamID не существует;
            string sql = "SELECT COUNT(*)" +
                         " FROM MANUALINPUTDEVPARAM WITH(NOLOCK)" +
                         " WHERE (" +
                         "        (MANUALINPUTPARAMID=@ManualInputParamID) AND" +
                         "        (DEV_ID=@Dev_ID)" +
                         "      )";

            long recordCount = 0;
            int count = 0;

            SqlConnection connection = DBConnections.Connection;
            connection.Open();

            try
            {
                SqlCommand command = new SqlCommand(sql, connection);
                command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;
                command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = dev_ID;

                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        recordCount = Convert.ToInt32(values[0]);

                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }

                switch (count)
                {
                    case 1:
                        return (recordCount == 0) ? false : true;

                    default:
                        //считано более одной записи
                        throw new Exception(string.Concat("CheckManualInputDevParamExist: ", string.Format(cReadedRecordNotSingle, count.ToString())));
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        public static void SaveToManualInputDevParam(int dev_ID, int manualInputParamID, double value)
        {
            //выполняет сохранение значения вручную созданного параметра применительно к изделию
            //если принятый manualInputParamID=null - выполняется попытка создания нового параметра, иначе выполняется редактирование параметра
            if (ExistManualInputDevParam(dev_ID, manualInputParamID))
            {
                //выполняется попытка редактирования значения параметра
                UpdateManualInputDevParam(dev_ID, manualInputParamID, value);                
            }
            else
            {
                //выполняется попытка создания новой записи
                InsertToManualInputDevParam(dev_ID, manualInputParamID, value);
            }
        }

        public static void DeleteFromManualInputDevParam(int devID, int manualInputParamID)
        {
            //удаление места хранения значения параметра с идентификатором devID, manualInputParamID из таблицы ManualInputDevParam
            SqlConnection connection = DBConnections.Connection;

            string sql = "DELETE" +
                         " FROM MANUALINPUTDEVPARAM" +
                         " WHERE (" +
                         "        (DEV_ID=@Dev_ID) AND" +
                         "        (MANUALINPUTPARAMID=@ManualInputParamID)" +
                         "       )";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = devID;
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void DeleteFromManualInputDevParam(int manualInputParamID)
        {
            //удаление всех мест хранения значений параметра с идентификатором manualInputParamID из таблицы ManualInputDevParam
            SqlConnection connection = DBConnections.Connection;

            string sql = "DELETE" +
                         " FROM MANUALINPUTDEVPARAM" +
                         " WHERE (MANUALINPUTPARAMID=@ManualInputParamID)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@ManualInputParamID", SqlDbType.Int).Value = manualInputParamID;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        #endregion

        #region DeviceComments

        public static void InsertToDeviceComments(int dev_ID, long userID, string comments)
        {
            //вставка новой записи в таблицу DEVICECOMMENTS
            SqlConnection connection = DBConnections.Connection;

            string sql = "INSERT INTO DEVICECOMMENTS(DEV_ID, USERID, RECORDDATE, COMMENTS)" +
                         " VALUES (@Dev_ID, @UserID, GETDATE(), @Comments)";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = dev_ID;
            command.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userID;
            command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = comments;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        private static void UpdateDeviceComments(int dev_ID, long userID, DateTime RecordDate, string comments)
        {
            //редактирование комментария в таблице DEVICECOMMENTS
            SqlConnection connection = DBConnections.Connection;

            string sql = "UPDATE DEVICECOMMENTS" +
                         " SET COMMENTS=@Comments," +
                         " WHERE (" +
                         "        (DEV_ID=@Dev_ID) AND" +
                         "        (USERID=@UserID) AND" +
                         "        (RECORDDATE=@RecordDate)" +
                         "       )";

            SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Dev_ID", SqlDbType.Int).Value = dev_ID;
            command.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userID;
            command.Parameters.Add("@RecordDate", SqlDbType.DateTime).Value = RecordDate;
            command.Parameters.Add("@Comments", SqlDbType.NVarChar).Value = comments;

            connection.Open();

            try
            {
                command.ExecuteNonQuery();
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        #endregion

        public static string DeviceCodeByDevID(int devID)
        {
            //чтение поля Devices.Code по принятому идентификатору изделия devID
            //возвращает:
            //null  - изделие devID не зарегистрировано;
            //не null - значение Devices.Code по принятому идентификатору изделия devID
            string deviceCode = null;

            SqlConnection connection = DBConnections.Connection;

            connection.Open();

            try
            {
                string sql = "SELECT CODE" +
                             " FROM DEVICES" +
                             string.Format(" WHERE (DEV_ID='{0}')", devID);

                int count = 0;
                string readedDeviceCode = null;

                SqlCommand command = new SqlCommand(sql, connection);
                SqlDataReader reader = command.ExecuteReader();

                try
                {
                    object[] values = new object[reader.FieldCount];

                    while (reader.Read())
                    {
                        reader.GetValues(values);

                        readedDeviceCode = Convert.ToString(values[0]);
                        count++;
                    }
                }

                finally
                {
                    reader.Close();
                }

                switch (count)
                {
                    case 0:
                        //изделия с идентификатором devID не существует
                        break;

                    case 1:
                        //пользователь userID имеет регистрацию в системе DC
                        deviceCode = readedDeviceCode;
                        break;

                    default:
                        //считано более одной записи для devID
                        throw new Exception(string.Format(cReadedRecordNotSingle, count.ToString(), "devID=" + devID.ToString()));
                }
            }

            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return deviceCode;
        }

    }
}
