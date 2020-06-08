using SCME.Types.Profiles;
using System;
using System.Globalization;

namespace SCME.dbViewer
{
    public static class Routines
    {
        public static TemperatureCondition TemperatureConditionByTemperature(double temperatureValue)
        {
            return (temperatureValue > 25) ? TemperatureCondition.TM : TemperatureCondition.RT;
        }

        public static bool TryStringToDouble(string value, out double dValue)
        {
            //double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture);
            return double.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dValue);
        }

        public static char SystemDecimalSeparator()
        {
            return Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
        }

        public static bool IsInteger(string value, out int iValue, out bool isDouble, out double dValue)
        {
            //если value есть целое число - вернёт true, иначе false
            //в isDouble вернёт признак того, что принятый value успешно преобразуется к типу double
            bool result = int.TryParse(value, out iValue);

            if (result)
            {
                isDouble = false;
                dValue = 0;
            }
            else
                isDouble = double.TryParse(value, out dValue);

            return result;

            /*
            if (isDouble)
                return Math.Abs(dValue % 1) <= (double.Epsilon * 100);
            else
                return false;
            */
        }

        public static bool IsBoolean(string value)
        {
            //используется для проверки описания норм на измеряемые параметры
            return ((value == "0") || (value == "1"));
        }

        public static int? MinDeviceClass(int? value1, int? value2)
        {
            int? result;

            if ((value1 == null) && (value2 == null))
            {
                //оба значения равны null, сравнивать нечего
                result = null;
            }
            else
            {
                if ((value1 != null) && (value2 != null))
                {
                    //оба значения не null
                    result = Math.Min((int)value1, (int)value2);
                }
                else
                {
                    //одно из значений не null, а другое null
                    result = null;
                }
            }

            return result;
        }

        public static object MaxInt(object value1, object value2)
        {
            object result;

            if ((value1 == DBNull.Value) && (value2 == DBNull.Value))
            {
                //оба значения равны null, сравнивать нечего
                result = DBNull.Value;
            }
            else
            {
                if ((value1 != DBNull.Value) && (value2 != DBNull.Value))
                {
                    //оба значения не null
                    result = Math.Max((int)value1, (int)value2);
                }
                else
                {
                    //одно из значений не null, а другое null
                    result = (value1 == DBNull.Value) ? value2 : value1;
                }
            }

            return result;
        }

        public static string NameOfHiddenColumn(string columnName)
        {
            //возвращает имя скрытого столбца, предназначенного для хранения дополнительного значения для столбца с именем columnName
            return string.Concat(columnName, Constants.HiddenMarker);
        }

        public static string NameOfUnitMeasure(string columnName)
        {
            //возвращает имя скрытого столбца, предназначенного для хранения единицы измерения для столбца с именем columnName
            return string.Concat(columnName, "UnitMeasure", Constants.HiddenMarker);
        }

        public static string NameOfNrmMinParametersColumn(string columnName)
        {
            //возвращает имя скрытого столбца, предназначенного для хранения нормы Min для столбца с именем columnName
            return string.Concat(columnName, "NrmMinParameters", Constants.HiddenMarker);
        }

        public static string NameOfNrmMaxParametersColumn(string columnName)
        {
            //возвращает имя скрытого столбца, предназначенного для хранения нормы Max для столбца с именем columnName
            return string.Concat(columnName, "NrmMaxParameters", Constants.HiddenMarker);
        }

        public static string NameOfIsPairCreatedColumn()
        {
            //возвращает имя скрытого столбца, предназначенного для хранения флага образования температурной пары
            return string.Concat(Constants.IsPairCreated, Constants.HiddenMarker);
        }

        public static string NameOfRecordIsStorageColumn()
        {
            //возвращает имя скрытого столбца, предназначенного для хранения флага об использовании записи для хранения данных от других записей
            return string.Concat(Constants.RecordIsStorage, Constants.HiddenMarker);
        }

        public static bool IsColumnHidden(string columnName)
        {
            //отвечает на вопрос является ли столбец с принятм именем скрытым столбцом
            return columnName.Contains(Constants.HiddenMarker);
        }

        public static string TrimEndNumbers(string value)
        {
            //вырезает все цифры начиная с конца принятого value и возвращает value без цифр
            char[] trimChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            return value.TrimEnd(trimChars);
        }

        public static string TestNameInDataGridColumn(string test)
        {
            return (test == "StaticLoses") ? "SL" : test;
        }      

        public static bool CheckBit(ulong value, byte bitNumber)
        {
            //проверка состояния бита с номером bitNumber в value
            ulong bitMask = (ulong)(1 << bitNumber);

            return (value & bitMask) != 0;
        }

        public static ulong SetBit(ulong value, byte bitNumber)
        {
            //установка бита с номером bitNumber в value
            ulong bitMask = (ulong)(1 << bitNumber);

            return (value | bitMask);
        }

        public static ulong DropBit(ulong value, byte bitNumber)
        {
            //сброс бита с номером bitNumber в value
            ulong bitMask = ~(ulong)(1 << bitNumber);

            return (value & bitMask);
        }

        public static bool IsUserAdmin(ulong permissionsLo)
        {
            //отвечает на вопрос является ли обладатель битовой маски permissionsLo правами администратора в данной системе. за это отвечает бит 0
            return CheckBit(permissionsLo, 0);
        }

        public static bool IsUserCanReadCreateComments(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo читать/создавать комментарии. за это отвечает бит 1
            return CheckBit(permissionsLo, 1);
        }

        public static bool IsUserCanReadComments(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo читать комментарии. за это отвечает бит 2
            return CheckBit(permissionsLo, 2);
        }

        public static bool IsUserCanCreateParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo создавать параметры в ManualInputParams
            return CheckBit(permissionsLo, 3);
        }

        public static bool IsUserCanEditParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo редактировать значения параметров созданных вручную в ManualInputParams
            return CheckBit(permissionsLo, 4);
        }

        public static bool IsUserCanDeleteParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo удалять параметры созданные вручную из ManualInputParams
            return CheckBit(permissionsLo, 5);
        }

        public static bool IsUserCanCreateValueOfManuallyEnteredParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo создавать места хранения значения параметра созданного вручную в ManualInputDevParam
            return CheckBit(permissionsLo, 6);
        }

        public static bool IsUserCanEditValueOfManuallyEnteredParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo редактировать значения параметров созданных вручную в ManualInputDevParam
            return CheckBit(permissionsLo, 7);
        }

        public static bool IsUserCanDeleteValueOfManuallyEnteredParameter(ulong permissionsLo)
        {
            //отвечает на вопрос может ли обладатель битовой маски permissionsLo удалять значения параметров созданных вручную в ManualInputDevParam
            return CheckBit(permissionsLo, 8);
        }

    }
}
