using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using SCME.Types;

namespace SCME.ProfileLoader
{
    public class Properties
    {
        public string MSSQLServer { get; set; }
        public string MSSQLDatabase { get; set; }
        public bool MSSQLIntegratedSecurity { get; set; }
        public int SQLTimeout { get; set; }
        public string MSSQLUserId { get; set; }
        public string MSSQLPassword { get; set; }
        public string SQLiteFileName { get; set; }
        public TypeDb TypeDb { get; set; }
        
        
        public static Properties Default{get;}
        
        
        
        static Properties()
        {
            Default = JsonConvert.DeserializeObject<Properties>(File.ReadAllText("appsettings.json"));
        }
    }
}
