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

        [HttpPost]
        public void Start(string mme, DateTime timestamp, bool debug, DateTime lastUpdate)
        {
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                Data1 = lastUpdate.ToBinary(),
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
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.HEART_BEAT_EVENT_NAME),
            });
            _db.SaveChanges();
        }
        
        
    }
}