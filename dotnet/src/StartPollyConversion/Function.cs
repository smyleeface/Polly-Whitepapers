using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using LambdaSharp;
using LambdaSharp.SimpleQueueService;
using Newtonsoft.Json;
using PollyWhitepapers.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollyWhitepapers.StartPollyConversion {
    
    static class StringExtensions {

        public static IEnumerable<String> SplitInParts(this String text, Int32 partLength) {
            if (text == null)
                throw new ArgumentNullException("text");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");
            for (var i = 0; i < text.Length; i += partLength)
                yield return text.Substring(i, Math.Min(partLength, text.Length - i));
        }

    }
    
    public class Function : ALambdaQueueFunction<QueueEvent>  {
        private IAmazonS3 _s3Client;
        private string _topicArn;
        private IAmazonPolly _pollyClient;

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) {
            _s3Client = new AmazonS3Client();
            _pollyClient = new AmazonPollyClient();
            _topicArn = config.ReadText("StartPollyConversionCompleteSns");
        }

        public override async Task ProcessMessageAsync(QueueEvent queueEvent) {
            LogInfo($"queueEvent {JsonConvert.SerializeObject(queueEvent)}");
            
            if (queueEvent.Event != null) {
                LogInfo($"testEvent {JsonConvert.SerializeObject(queueEvent)}");
                return;
            }
            
            // read results from s3 bucket
            var bucket = queueEvent.Records.FirstOrDefault()?.S3.Bucket.Name;
            var plaintextFile = queueEvent.Records.FirstOrDefault()?.S3.Object.Key;
            var jobId = plaintextFile.Split('/')[1];
            var getPlaintextRequest = new GetObjectRequest {
                Key = plaintextFile,
                BucketName = bucket
            };
            var getPlaintextResponse = await _s3Client.GetObjectAsync(getPlaintextRequest);
            var plaintextList = new List<string>();
            using (var streamReader = new StreamReader(getPlaintextResponse.ResponseStream)) {
                plaintextList = (streamReader.ReadToEnd()).SplitInParts(100000).ToList();
            }
            LogInfo($"plaintextList {JsonConvert.SerializeObject(plaintextList)}");
            
            foreach (var plaintextString in plaintextList) {

                // send text to polly
                var startPollyRequest = new StartSpeechSynthesisTaskRequest {
                    Text = plaintextString,
                    VoiceId = VoiceId.Kendra,
                    OutputFormat = OutputFormat.Mp3,
                    OutputS3BucketName = bucket,
                    OutputS3KeyPrefix = $"mp3/{jobId}/",
                    SnsTopicArn = _topicArn
                };
                var response = await _pollyClient.StartSpeechSynthesisTaskAsync(startPollyRequest);
                LogInfo($"response {JsonConvert.SerializeObject(response)}");
            }
        }
    }
}
