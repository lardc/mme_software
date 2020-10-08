namespace SCME.MEFAServer.Tables
{
    public class MonitoringEventType
    {
        public const string HeartBeatEventName = "MME_HEART_BEAT";
        public const string StartEventName = "MME_START";
        public const string TestEventName = "MME_TEST";
        public const string ErrorEventName = "MME_ERROR";
        public const string SyncEventName = "MME_SYNC";
            
        public int Id { get; set; }
        public string EventName { get; set; }
    }
}