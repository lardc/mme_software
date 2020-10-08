using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SCME.MEFAServer.Tables;

namespace SCME.MEFAServer
{
    public class MonitoringContext :DbContext
    {
        public DbSet<MonitoringEvent> MonitoringEvents { get; set; }
        public DbSet<MonitoringEventType> MonitoringEventTypes { get; set; }
        public DbSet<MonitoringStat> MonitoringStats { get; set; }
        public DbSet<MonitoringStatType> MonitoringStatTypes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured == false)
                optionsBuilder.UseSqlServer(File.ReadAllText("EFMigrateConnectionString.txt"));
            else
                base.OnConfiguring(optionsBuilder);
        }

        public void AddDataToTableType()
        {
            var monitoringEventTypes = new[]
            {
                new MonitoringEventType()
                {
                    EventName = MonitoringEventType.StartEventName
                },
                new MonitoringEventType()
                {
                    EventName = MonitoringEventType.TestEventName
                },
                new MonitoringEventType()
                {
                    EventName = MonitoringEventType.ErrorEventName
                },
                new MonitoringEventType()
                {
                    EventName = MonitoringEventType.SyncEventName
                },
                new MonitoringEventType()
                {
                    EventName = MonitoringEventType.HeartBeatEventName
                },
            };

            var monitoringStatTypes = new[]
            {
                new MonitoringStatType()
                {
                    StatName = "LASTSTART_HOURS"
                },
                new MonitoringStatType()
                {
                    StatName = "DAY_HOURS"
                },
                new MonitoringStatType()
                {
                    StatName = "TOTAL_HOURS"
                }
            };

            foreach (var i in monitoringEventTypes)
                if (MonitoringEventTypes.SingleOrDefault(m => m.EventName == i.EventName) == null)
                    MonitoringEventTypes.Add(i);
            
            foreach (var i in monitoringStatTypes)
                if (MonitoringStatTypes.SingleOrDefault(m => m.StatName == i.StatName) == null)
                    MonitoringStatTypes.Add(i);

            SaveChanges();

        }
        
        public MonitoringContext(string connectionString) : base()
        {
            
        }
        
        public MonitoringContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
            
    }
}