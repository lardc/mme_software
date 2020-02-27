// ReSharper disable UnusedAutoPropertyAccessor.Global

using Microsoft.Extensions.Configuration;

namespace SCME.UpdateServer
{

    public class MmeParameter
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public IConfigurationSection Configs { get; set; }
    }
    
    public class UpdateDataConfig
    {
        public string DataPathRoot { get; set; }
        public string ScmeAgentFolderName { get; set; }
        public string ScmeAgentExeName { get; set; }
        
        public string ScmeServiceConfigName { get; set; }
        
        public string ScmeCommonConfigName { get; set; }
        public string ScmeUiConfigName { get; set; }

        public string ScmeUIExeName { get; set; }
        public MmeParameter[] MmeParameters { get; set; }
        
    }
}