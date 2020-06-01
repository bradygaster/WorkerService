using System;
using System.Text.Json.Serialization;

namespace WorkerService
{
    public class InstanceInfo
    {
        [JsonPropertyName("hostName")]
        public string HostName { get; set; }
        
        [JsonPropertyName("hostTimeStamp")]
        public DateTime HostTimeStamp { get; set; }
    }
}