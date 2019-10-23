using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCME.Types.Profiles
{
    public enum TemperatureCondition
    {
        None = 0,
        RT = 1,
        TM = 2
    }

    public static class ProfileRoutines
    {
        public static SubjectForMeasure CalcSubjectForMeasure(string profileName)
        {
            //вычисляет по коду профиля profileName предмет измерения
            var result = Types.SubjectForMeasure.Unknown;

            if (profileName != null)
            {
                profileName = profileName.Trim();

                if (profileName != string.Empty)
                {
                    string subjectForMeasure = profileName.Substring(0, 3);
                    Enum.TryParse(subjectForMeasure, true, out result);
                }
            }

            return result;
        }

        private static TemperatureCondition TemperatureConditionByProfileName(string profileName)
        {
            TemperatureCondition result = TemperatureCondition.None;

            if (profileName.Trim() != string.Empty)
            {
                string profileNameUpper = profileName.ToUpper();

                result = profileNameUpper.Contains("RT") ? TemperatureCondition.RT : profileNameUpper.Contains("TM") ? TemperatureCondition.TM : TemperatureCondition.None;
            }

            return result;
        }

        public static string ProfileBodyByProfileName(string profileName)
        {
            //вычисляем тело по принятому profileName
            string result = profileName.ToUpper();

            int start = result.IndexOf(" ", 0);
            if (start == -1)
                return string.Empty;

            start = result.IndexOf(" ", start + 1);
            if (start == -1)
                return string.Empty;

            start = result.IndexOf(" ", start + 1);
            if (start == -1)
                return string.Empty;

            //вычисляем индекс 4-го пробела (за которым начинается спецтребование)
            int specialReqStart = result.IndexOf(" ", start + 1);

            string specialReq = string.Empty;

            if (specialReqStart != -1)
                specialReq = result.Substring(specialReqStart);

            result = "*" + result.Substring(3, start - 3) + " *";

            if (specialReq != string.Empty)
                result += specialReq;

            return result;
        }

        public static string SpecialMarkByProfileBody(string profileBody)
        {
            //извлекает обозначение спецмаркировки из обозначения тела профиля profileBody - оно располагается сразу за второй * в profileBody
            int start = profileBody.IndexOf("*", 0);
            if (start == -1)
                return string.Empty;

            start = profileBody.IndexOf("*", start + 1);
            if (start == -1)
            {
                return string.Empty;
            }
            else return profileBody.Substring(start);

        }

        public static string SpecialMarkByProfileName(string profileName)
        {
            //извлекает обозначение спецмаркировки из обозначения профиля ProfileName. она начинается после 4-го пробела
            //PSETm Т253 1390 24 GvA 03 08
            return string.Empty;

        }

        public static string MakeRT(string profileName)
        {
            return System.Text.RegularExpressions.Regex.Replace(profileName, "TM", "RT", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static string MakeTM(string profileName)
        {
            return System.Text.RegularExpressions.Regex.Replace(profileName, "RT", "TM", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static string ProfileRTBodyByProfileName(string profileName)
        {
            string result = ProfileBodyByProfileName(profileName);

            switch (TemperatureConditionByProfileName(result))
            {
                case TemperatureCondition.TM:
                    //меняем TM на RT
                    result = MakeRT(result);
                    break;
            }

            return result;
        }

        public static string ProfileTMBodyByProfileName(string profileName)
        {
            string result = ProfileBodyByProfileName(profileName);

            switch (TemperatureConditionByProfileName(result))
            {
                case TemperatureCondition.RT:
                    //меняем в profName RT на TM
                    result = MakeTM(result);
                    break;
            }

            return result;
        }

        public static string PairProfileBodyByProfileName(string profileName)
        {
            //вычисляет тело-пару по принятому profileName
            switch (TemperatureConditionByProfileName(profileName))
            {
                case TemperatureCondition.RT:
                    //меняем в profName RT на TM
                    return ProfileTMBodyByProfileName(profileName);

                case TemperatureCondition.TM:
                    //меняем TM на RT
                    return ProfileRTBodyByProfileName(profileName); ;

                default:
                    return ProfileBodyByProfileName(profileName);
            }
        }

        public static int? DeviceClassByProfileName(string profileName)
        {
            //извлечение класса из наименования профиля. класс в наименовании профиля записан целым числом начиная с символа, следующего за 3-ым пробелом до 4-го пробела при его наличии, если же 4-го пробела нет - то до конца строки
            string profileNameUpper = profileName.ToUpper();
            const string delimeter = " ";

            int start = 0;
            for (int i = 1; i <= 3; i++)
            {
                start = profileNameUpper.IndexOf(delimeter, start);

                if (start == -1)
                    return null;
                else start++;
            }

            int end = profileNameUpper.IndexOf(delimeter, start);

            string cl = (end == -1) ? profileNameUpper.Substring(start) : profileNameUpper.Substring(start, end - start);

            int result;

            switch (int.TryParse(cl, out result))
            {
                case true:
                    return result;

                default:
                    return null;
            }
        }
    }
}
