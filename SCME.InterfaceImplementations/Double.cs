using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SCME.InterfaceImplementations
{
    public static class Double
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
                /*File.AppendAllText("CreateCultureError", $"{DateTime.Now}{Environment.NewLine}{ex.ToString()}");*/
                return null;
            }
            
        }

        static Double()
        {
            foreach (var i in new string[] {"ru-RU", "en-EN" ,"en-US"})
                cultures[i] = CreateCulture(i);
        }


        public static double ParseInternationally(string str)
        {
            try
            {
                return double.Parse(str.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
            foreach (var i in cultures.Select(m => m.Value).Where(i => i != null))
                if (double.TryParse(str, NumberStyles.Any, i, out var result))
                    return result;
            
            throw new NotImplementedException($"cant` parse string {str}");  
        }
    }
}