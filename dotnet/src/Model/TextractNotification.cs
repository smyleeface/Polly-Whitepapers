using Amazon.Textract;
using Newtonsoft.Json;

namespace PollyWhitepapers.Model {
    public class TextractNotification {
        
        [JsonProperty("JobId")]
        public string JobId;
        
        [JsonProperty("Status")]
        public JobStatus Status;
        
        [JsonProperty("API")]
        public string Api;
        
        [JsonProperty("Timestamp")]
        public string Timestamp;
        
        [JsonProperty("DocumentLocation")]
        public Amazon.Textract.Model.S3Object DocumentLocation;
    }
}