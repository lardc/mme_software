namespace SCME.MEFADB.Tables
{
    public class MonitoringEventType
    {
        public const string START_EVENT_NAME = "MME_START";
        public const string TEST_EVENT_NAME = "MME_TEST";
        public const string ERROR_EVENT_NAME = "MME_ERROR";
        public const string SYNC_EVENT_NAME = "MME_SYNC";

        public int Id { get; set; }
        public string EventName { get; set; }
    }
}