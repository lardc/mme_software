﻿using System;
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

            result = result.Substring(3, start - 3);

            return result;
        }

        public static string MakeRT(string ProfileName)
        {
            return System.Text.RegularExpressions.Regex.Replace(ProfileName, "TM", "RT", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static string MakeTM(string ProfileName)
        {
            return System.Text.RegularExpressions.Regex.Replace(ProfileName, "RT", "TM", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
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
            //извлечение класса из наименования профиля. класс в наименовании профиля записан целым числом начиная с символа, следующего за 3-ым пробелом до 4-го пробела
            string profileNameUpper = profileName.ToUpper();

            int start = 0;
            for (int i = 1; i <= 3; i++)
            {
                start = profileNameUpper.IndexOf(" ", start);

                if (start == -1)
                    return null;
                else start++;
            }

            int end = profileNameUpper.IndexOf(" ", start);

            string cl = profileNameUpper.Substring(start, end - start);

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
