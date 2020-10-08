using System;
using System.ComponentModel.DataAnnotations;

namespace SCME.MEFAServer.Tables
{
    public class MonitoringEvent
    {
        public int Id { get; set; }
        
        public int MonitoringEventTypeId { get; set; }
        
        public string MmeCode { get; set; }
        
        public DateTime Timestamp { get; set; }

        public long Data1 { get; set; }
        public long Data2 { get; set; }
        public long Data3 { get; set; }
        [MaxLength]
        public string Data4 { get; set; }
        
        public MonitoringEventType MonitoringEventType { get; set; }
    }
} 