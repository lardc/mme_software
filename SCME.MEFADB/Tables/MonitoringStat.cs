using System;

namespace SCME.MEFADB.Tables
{
    public class MonitoringStat
    {
        public int Id { get; set; }
        
        public int MonitoringStatTypeId { get; set; }

        public string MmeCode { get; set; }

        public DateTime KeyData { get; set; }

        public int ValueData { get; set; }

        
        public MonitoringStatType MonitoringStatType { get; set; }
    }
}