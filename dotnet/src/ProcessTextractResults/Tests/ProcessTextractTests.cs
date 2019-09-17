using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Amazon.Textract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PollyWhitepapers.Model;
using Xunit;

namespace PollyWhitepapers.ProcessTextractResults.Tests {
    public class ProcessTextractTests {

        [Fact]
        public void Process_Json() {
            string result = "";
            string text = File.ReadAllText(@"/Users/pattyr/Downloads/results_1.json");
            using (var memoryStream = new MemoryStream()) {
                using (var streamWriter = new StreamWriter(memoryStream)) {
                    streamWriter.Write(text);
                    streamWriter.Flush();
                    memoryStream.Position = 0;
                    using (var streamReader = new StreamReader(memoryStream)) {
                        var contents = streamReader.ReadToEnd();
                        var contents2 = JsonConvert.DeserializeObject<TextractResults>(contents);
                        var blocks = contents2.Blocks;
                        foreach (var block in blocks) {
                            if (block.BlockType == BlockType.LINE) {
                                result += block.Text;
                            }
                        }
                        // var contents2 = JsonConvert.DeserializeObject<IEnumerable<Amazon.Textract.Model.Block>>(contents);
                        Console.Write("hello");
                    }
                }

            }
        }
        
    }
}