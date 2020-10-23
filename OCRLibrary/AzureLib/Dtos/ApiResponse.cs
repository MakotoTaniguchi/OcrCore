using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace OCRLibrary.AzureLib.Dtos
{
    [JsonObject]
    public class ApiResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createdDateTime")]
        public DateTime CreatedDateTime { get; set; }

        [JsonProperty("lastUpdatedDateTime")]
        public DateTime LastUpdatedDateTime { get; set; }

        [JsonProperty("analyzeResult")]
        public AnalyzeResult AnalyzeResult { get; set; }
        
    }

    [JsonObject]
    public class AnalyzeResult
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("readResults")]
        public List<ReadResult> ReadResults { get; set; }

    }

    [JsonObject]
    public class ReadResult
    {
        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("angle")]
        public double Angle { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("lines")]
        public List<Line> Lines { get; set; }

    }

    [JsonObject]
    public class Line
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("boundingBox")]
        public List<int> BoundingBox { get; set; }

        [JsonProperty("Words")]
        public List<Word> Words { get; set; }
    }

    [JsonObject]
    public class Word
    {
        [JsonProperty("boundingBox")]
        public List<int> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
