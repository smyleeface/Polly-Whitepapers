using Amazon.Lambda.S3Events;
using Newtonsoft.Json;

namespace PollyWhitepapers.Model {

    public class QueueEvent : S3Event {
        [JsonProperty("Service", NullValueHandling = NullValueHandling.Ignore)]
        public string Service;
        
        [JsonProperty("Event", NullValueHandling = NullValueHandling.Include)]
        public string Event;
        
        [JsonProperty("Time", NullValueHandling = NullValueHandling.Ignore)]
        public string Time;
        
        [JsonProperty("Bucket", NullValueHandling = NullValueHandling.Ignore)]
        public string Bucket;
        
        [JsonProperty("RequestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId;
        
        [JsonProperty("HostId", NullValueHandling = NullValueHandling.Ignore)]
        public string HostId;
    }
}
