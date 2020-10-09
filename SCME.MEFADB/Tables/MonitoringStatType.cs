namespace SCME.MEFADB.Tables
{
    public class MonitoringStatType
    {
        public const string LAST_START_HOURS = "LAST_START_HOURS";
        public const string DAY_HOURS = "DAY_HOURS";
        public const string TOTAL_HOURS = "TOTAL_HOURS";

        public int Id { get; set; }
        public string StatName { get; set; }
    }
}