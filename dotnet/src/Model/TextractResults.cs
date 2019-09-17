using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.S3Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PollyWhitepapers.Model {

    public class TextractResults {
        [JsonProperty("Blocks")]
        public List<Amazon.Textract.Model.Block> Blocks;
        
        [JsonProperty("DocumentMetadata")]
        public Amazon.Textract.Model.DocumentMetadata DocumentMetadata;
        
        [JsonProperty("JobStatus")]
        public TextractJobStatus JobStatus;
        
        [JsonProperty("NextToken")]
        public string NextToken;
        
        [JsonProperty("StatusMessage")]
        public string StatusMessage;

        [JsonProperty("ContentLength")]
        public int ContentLength;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("HttpStatusCode")]
        public HttpStatusCode HttpStatusCode;
    }
}
