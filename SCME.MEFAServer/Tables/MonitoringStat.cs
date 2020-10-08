namespace SCME.MEFAServer.Tables
{
    public class MonitoringStat
    {
        public int Id { get; set; }
        
        public int MonitoringStatTypeId { get; set; }

        public string MmeCode { get; set; }

        public int KeyData { get; set; }

        public string ValueData { get; set; }

        
        public MonitoringStatType MonitoringStatType { get; set; }
    }
}