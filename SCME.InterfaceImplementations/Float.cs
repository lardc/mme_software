using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SCME.InterfaceImplementations
{
    public static class Float
    {
        static CultureInfo ruRu;
        static CultureInfo enEn;
        static CultureInfo enUS;

        private static readonly Dictionary<string, CultureInfo> cultures = new Dictionary<string, CultureInfo>();

        private static CultureInfo  CreateCulture(string cultureName)
        {
            try
            {
                return new CultureInfo(cultureName);
            }
            catch (Exception ex)
            {
                File.AppendAllText("CreateCultureError", $"{DateTime.Now}{Environment.NewLine}{ex.ToString()}");
                return null;
            }
            
        }

        static Float()
        {
            foreach (var i in new string[] {"ru-RU", "en-EN" ,"en-US"})
                cultures[i] = CreateCulture(i);
        }


        public static float ParseInternationally(string str)
        {
            foreach (var i in cultures.Select(m => m.Value).Where(i => i != null))
                if (float.TryParse(str, NumberStyles.Any, i, out var result))
                    return result;
            
            throw new NotImplementedException($"cant` parse string {str}");  
        }
    }
}