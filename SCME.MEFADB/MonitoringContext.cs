using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SCME.MEFADB.Tables;

namespace SCME.MEFADB
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

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var n = 0;
            foreach (var i in new[]{MonitoringEventType.ERROR_EVENT_NAME, MonitoringEventType.START_EVENT_NAME, MonitoringEventType.SYNC_EVENT_NAME, MonitoringEventType.TEST_EVENT_NAME, MonitoringEventType.HEART_BEAT_EVENT_NAME})
                modelBuilder.Entity<MonitoringEventType>().HasData(new MonitoringEventType() {Id = ++n, EventName = i});

            n = 0;
            foreach (var i in new[]{MonitoringStatType.DAY_HOURS, MonitoringStatType.TOTAL_HOURS, MonitoringStatType.LAST_START_HOURS})
                modelBuilder.Entity<MonitoringStatType>().HasData(new MonitoringStatType() {Id = ++n, StatName = i});

        }

        public MonitoringContext()
        {
            
        }
        
        public MonitoringContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }
            
    }
}