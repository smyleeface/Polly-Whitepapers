using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.S3Events;
using Amazon.Textract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PollyWhitepapers.Model {

    public class TextractJobStatus {
            
        [JsonProperty("Value")]
        public JobStatus Value;
    }
}
