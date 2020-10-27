using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SCME.MEFADB;
using SCME.MEFADB.Tables;

namespace SCME.MEFAServer.Controllers
{
    
    [Route("[controller]/[action]")]
    public class EventController: ControllerBase
    {
        private MonitoringContext _db;

        public EventController(MonitoringContext db)
        {
            _db = db;
        }

        private MonitoringStat GetLastDay(string mme, DateTime timestamp)
        {
            var day = _db.MonitoringStats.SingleOrDefault(m => m.MmeCode == mme && m.MonitoringStatType.StatName == MonitoringStatType.DAY_HOURS && m.KeyData.Date == timestamp.Date);
            if (day != null) 
                return day;
            {
                day = new MonitoringStat()
                {
                    KeyData = timestamp.Date,
                    MmeCode = mme,
                    ValueData = 0,
                    MonitoringStatType = _db.MonitoringStatTypes.Single(m => m.StatName == MonitoringStatType.DAY_HOURS)
                };
                _db.MonitoringStats.Add(day);
            }

            return day;
        }

        [HttpPost]
        public void Start(string mme, DateTime timestamp, bool debug, DateTime lastUpdate, string softVersion)
        {
            var last = _db.MonitoringStats.Single(m => m.MmeCode == mme && m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS);
            last.KeyData = timestamp;
            last.ValueData = 0;

            GetLastDay(mme, timestamp);
            
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                Data1 = lastUpdate.ToBinary(),
                Data4 = softVersion,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.START_EVENT_NAME),
            });
            _db.SaveChanges();
        }

        public void Test(string mme, DateTime timestamp, Guid profileGuid, long devId)
        {
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.TEST_EVENT_NAME),
                Data2 = devId,
                Data4 = profileGuid.ToString("N")
            });
            _db.SaveChanges();
        }

        public void Error(string mme, DateTime timestamp, string error)
        {
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.ERROR_EVENT_NAME),
                Data4 = error
            });
            _db.SaveChanges();
        }
        
        public void SyncComplete(string mme, DateTime timestamp, int profilesCount)
        {
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.SYNC_EVENT_NAME),
                Data1 = profilesCount
            });
            _db.SaveChanges();
        }
        
        public void HeartBeat(string mme, DateTime timestamp)
        {
            foreach (var i in  _db.MonitoringStats.Where(m => m.MmeCode == mme && (m.MonitoringStatType.StatName == MonitoringStatType.TOTAL_HOURS || m.MonitoringStatType.StatName == MonitoringStatType.LAST_START_HOURS)))
                i.ValueData++;
            
            var day =  GetLastDay(mme, timestamp);
            day.ValueData++;
       
            _db.SaveChanges();
        }
        
        
    }
}