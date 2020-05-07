namespace SCME.Agent
{
    public class ConfigData
    {
        public string ServiceAppPath { get; set; }

        public string UIAppPath { get; set; }

        public string ProxyAppPath { get; set; }

        public bool IsProxyEnabled { get; set; }
        
        public int  ProxyInitDelayMs { get; set; }

        public bool DebugUpdate { get; set; }
        
        public string UpdateServerUrl { get; set; }

        public bool IsUserInterfaceEnabled { get; set; }
        public string MMECode { get; set; }
    }
}