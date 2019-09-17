using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using LambdaSharp;
using LambdaSharp.SimpleQueueService;
using Newtonsoft.Json;
using PollyWhitepapers.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollyWhitepapers.ProcessTextractResults {
    
    public class Function : ALambdaQueueFunction<QueueEvent>  {
        private IAmazonS3 _s3Client;

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) {
            _s3Client = new AmazonS3Client();
        }

        public override async Task ProcessMessageAsync(QueueEvent queueEvent) {
            LogInfo($"queueEvent {JsonConvert.SerializeObject(queueEvent)}");
            
            if (queueEvent.Event != null) {
                LogInfo($"testEvent {JsonConvert.SerializeObject(queueEvent)}");
                return;
            }
            
            // read manifest from s3 bucket
            var bucket = queueEvent.Records.FirstOrDefault()?.S3.Bucket.Name;
            var manifestKey = queueEvent.Records.FirstOrDefault()?.S3.Object.Key;
            var jobId = manifestKey.Split('/')[1];
            var getManifestRequest = new GetObjectRequest {
                Key = manifestKey,
                BucketName = bucket
            };
            var getManifestResults = await _s3Client.GetObjectAsync(getManifestRequest);
            var manifest = "";
            using (var streamReader = new StreamReader(getManifestResults.ResponseStream)) {
                manifest = Regex.Replace(streamReader.ReadToEnd(), @"\t|\n|\r", "");
            }
            LogInfo($"manifest {JsonConvert.SerializeObject(manifest)}");
            
            // process the results from textract results
            string result = "";
            foreach (var filePath in manifest.Split(';')) {
                var getResultsRequest = new GetObjectRequest {
                    Key = filePath,
                    BucketName = bucket
                };
                LogInfo($"getResultsRequest {JsonConvert.SerializeObject(getResultsRequest)}");
                var results = await _s3Client.GetObjectAsync(getResultsRequest);
                using (var streamReader = new StreamReader(results.ResponseStream)) {
                    var contents = JsonConvert.DeserializeObject<TextractResults>(streamReader.ReadToEnd());
                    var blocks = contents.Blocks;
                    foreach (var block in blocks) {
                        if (block.BlockType == BlockType.LINE) {
                            result += " " + block.Text;
                        }
                    }
                }
            }
            
            // write results to file
            using (var memoryStream = new MemoryStream()) {
                using (var streamWriter = new StreamWriter(memoryStream)) {
                    streamWriter.Write(result);
                    streamWriter.Flush();
                    memoryStream.Position = 0;
                    var s3ManifestResponse = await _s3Client.PutObjectAsync(new PutObjectRequest {
                        BucketName = bucket,
                        Key = $"processed/{jobId}/plaintext.txt", 
                        InputStream = memoryStream
                    });
                    if (s3ManifestResponse == null || s3ManifestResponse.HttpStatusCode != (HttpStatusCode) 200) {
                        throw new Exception("Unable to save plaintext file to s3");
                    }
                }
            }
        }
    }
}
