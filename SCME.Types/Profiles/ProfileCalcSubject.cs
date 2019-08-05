using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCME.Types.Profiles
{
    public class ProfileCalcSubject
    {
        public SubjectForMeasure CalcSubjectForMeasure(string profileName)
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
    }
}
