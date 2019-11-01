using System;

namespace SCME.Types.Commutation
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumUseRCC : Attribute
    {
        private readonly bool _value;

        public bool Value
        {
            get
            {
                return _value;
            }
        }

        public EnumUseRCC(bool value)
        {
            _value = value;
        }
    }

    public static class AttributesHelper
    {
        //возвращает значение применимости теста RCC к данному типу коммутации
        public static bool GetRCCUseAttributeValue(Enum value)
        {
            var result = false;

            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());

            if (!ReferenceEquals(fieldInfo, null))
            {
                //Get the stringvalue attributes  
                var attributes = fieldInfo.GetCustomAttributes(typeof(EnumUseRCC), false) as EnumUseRCC[];

                if (!ReferenceEquals(attributes, null))
                {
                    //Return the first if there was a match.  
                    result = attributes.Length > 0 ? attributes[0].Value : false;
                }
            }

            return result;
        }
    }
}
