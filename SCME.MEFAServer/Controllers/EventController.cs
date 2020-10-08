using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SCME.MEFAServer.Tables;

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
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.StartEventName),
            });
            _db.SaveChanges();
        }

        public void Test(string mme, DateTime timestamp, Guid profileGuid, long devId)
        {
            _db.MonitoringEvents.Add(new MonitoringEvent()
            {
                MmeCode = mme,
                Timestamp = timestamp,
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.TestEventName),
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
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.ErrorEventName),
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
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.SyncEventName),
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
                MonitoringEventType = _db.MonitoringEventTypes.Single(m => m.EventName == MonitoringEventType.HeartBeatEventName),
            });
            _db.SaveChanges();
        }
        
        
    }
}