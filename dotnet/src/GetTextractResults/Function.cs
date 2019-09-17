using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using Amazon.Textract.Model;
using LambdaSharp;
using LambdaSharp.SimpleQueueService;
using Newtonsoft.Json;
using PollyWhitepapers.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PollyWhitepapers.GetTextractResults {

    public class Function : ALambdaQueueFunction<TextractNotification>  {
        private IAmazonTextract _textractClient;
        private IAmazonS3 _s3Client;
        private string _bucketName;

        //--- Methods ---
        public override async Task InitializeAsync(LambdaConfig config) {
            _textractClient = new AmazonTextractClient();
            _s3Client = new AmazonS3Client();
            _bucketName = AwsConverters.ConvertBucketArnToName(config.ReadText("FileBucket"));
        }

        public override async Task ProcessMessageAsync(TextractNotification queueEvent) {
            LogInfo($"notificationMessage {JsonConvert.SerializeObject(queueEvent)}");
            if (queueEvent.Status == JobStatus.SUCCEEDED) {
                try {
                    var jobId = queueEvent.JobId;
                    string nextToken = null;
                    var counter = 0;
                    var manifestContents = new List<string>();
                    do {
                        // get the result from the job
                        var getRequest = new GetDocumentAnalysisRequest {
                            JobId = jobId,
                            NextToken = nextToken
                        };
                        LogInfo(JsonConvert.SerializeObject(getRequest));
                        var response = await _textractClient.GetDocumentAnalysisAsync(getRequest);
                        // LogInfo(JsonConvert.SerializeObject(response));
                    
                        // save response to s3
                        var filePath = $"results/{jobId}/results_{counter}.json";
                        using (var memoryStream = new MemoryStream()) {
                            using (var streamWriter = new StreamWriter(memoryStream)) {
                                streamWriter.Write(JsonConvert.SerializeObject(response));
                                streamWriter.Flush();
                                var s3Response = await _s3Client.PutObjectAsync(new PutObjectRequest {
                                    BucketName = _bucketName, 
                                    Key = filePath, 
                                    InputStream = memoryStream
                                });
                                if (s3Response == null || s3Response.HttpStatusCode != (HttpStatusCode) 200) {
                                    throw new Exception("Unable to save results file to s3");
                                }
                            }
                        }

                        // add to the manifest
                        manifestContents.Add(filePath);
                        
                        // get the next response token
                        nextToken = response.NextToken;
                        counter++;
                    } while (nextToken != null);

                    var manifestContentsString = string.Join(';', manifestContents);
                    LogInfo(manifestContentsString);
                    
                    // write manifest
                    using (var memoryStream1 = new MemoryStream()) {
                        using (var streamWriter1 = new StreamWriter(memoryStream1)) {
                            streamWriter1.WriteLine(manifestContentsString);
                            streamWriter1.Flush();
                            var s3ManifestResponse = await _s3Client.PutObjectAsync(new PutObjectRequest {
                                BucketName = _bucketName,
                                Key = $"results/{jobId}/manifest", 
                                InputStream = memoryStream1
                            });
                            if (s3ManifestResponse == null || s3ManifestResponse.HttpStatusCode != (HttpStatusCode) 200) {
                                throw new Exception("Unable to save manifest file to s3");
                            }
                        }
                    }
                    Thread.Sleep(100);
                    
                }
                catch (Exception e) {
                    LogError(e);
                }  
            }
        }
    }
}
