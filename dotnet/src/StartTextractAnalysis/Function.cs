using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Textract;
using Amazon.Textract.Model;
using LambdaSharp;
using LambdaSharp.SimpleQueueService;
using Newtonsoft.Json;
using PollyWhitepapers.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollyWhitepapers.StartTextractAnalysis {

    public class Function : ALambdaQueueFunction<QueueEvent>  {
        private IAmazonTextract _textractClient;
        private string _roleArn;
        private string _topicArn;

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) {
            _textractClient = new AmazonTextractClient();
            _roleArn = config.ReadText("StartTextractAnalysisCompleteSnsRole");
            _topicArn = config.ReadText("StartTextractAnalysisCompleteSns");
        }

        public override async Task ProcessMessageAsync(QueueEvent queueEvent) {
            LogInfo($"queueEvent {JsonConvert.SerializeObject(queueEvent)}");

            if (queueEvent.Event != null) {
                LogInfo($"testEvent {JsonConvert.SerializeObject(queueEvent)}");
                return;
            }
            
            var bucket = queueEvent.Records.FirstOrDefault()?.S3.Bucket.Name;
            var objectKey = queueEvent.Records.FirstOrDefault()?.S3.Object.Key;
            var startRequest = new StartDocumentAnalysisRequest {
                DocumentLocation = new DocumentLocation {
                    S3Object = new S3Object {
                        Bucket = bucket,
                        Name = objectKey
                    }
                },
                NotificationChannel = new NotificationChannel {
                    RoleArn = _roleArn,
                    SNSTopicArn = _topicArn
                },
                FeatureTypes = new List<string> {
                    "TABLES"
                }
            };
            LogInfo(JsonConvert.SerializeObject(startRequest));
            var response = await _textractClient.StartDocumentAnalysisAsync(startRequest);
            LogInfo(JsonConvert.SerializeObject(response));
        }
    }
}
