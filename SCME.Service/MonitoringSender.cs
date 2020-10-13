using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace SCME.Service
{
    public class MonitoringSender
    {
        private readonly string _mme;
        private readonly bool _debug;
        private readonly DateTime _lastUpdate;
        private readonly Uri _baseUri;
        
        public MonitoringSender(string baseUri, string mme, bool debug, DateTime lastUpdate)
        {
            _mme = mme;
            _debug = debug;
            _lastUpdate = lastUpdate;
            _baseUri = new Uri(baseUri);
        }
        
        public void Start()
        {
            var client = new HttpClient();
            var uri = new Uri(_baseUri, "Event/Start");
            client.PostAsync(uri, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mme", _mme),
                new KeyValuePair<string, string>("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("debug", _debug.ToString()),
                new KeyValuePair<string, string>("lastUpdate", _lastUpdate.ToString(CultureInfo.InvariantCulture)),
            }));
        }

        public void Sync(int profilesCount)
        {
            var client = new HttpClient();
            var uri = new Uri(_baseUri, "Event/Sync");
            client.PostAsync(uri, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mme", _mme),
                new KeyValuePair<string, string>("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("profilesCount", profilesCount.ToString())
            }));
        }
        
        public void Test(Guid profileGuid, long devId)
        {
            var client = new HttpClient();
            var uri = new Uri(_baseUri, "Event/Test");
            client.PostAsync(uri, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mme", _mme),
                new KeyValuePair<string, string>("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("profileGuid", profileGuid.ToString("N")),
                new KeyValuePair<string, string>("devId", devId.ToString())
            }));
        }
        
        public void HardwareError(string error)
        {
            var client = new HttpClient();
            var uri = new Uri(_baseUri, "Event/Error");
            client.PostAsync(uri, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mme", _mme),
                new KeyValuePair<string, string>("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("error", error)
            }));
        }
        
        public void HeartBeat()
        {
            var client = new HttpClient();
            var uri = new Uri(_baseUri, "Event/HeartBeat");
            client.PostAsync(uri, new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mme", _mme),
                new KeyValuePair<string, string>("timestamp", DateTime.Now.ToString(CultureInfo.InvariantCulture)),
            }));
        }
        
    }
}