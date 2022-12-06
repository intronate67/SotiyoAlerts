using Newtonsoft.Json;

namespace SotiyoAlerts.Models.Eve
{
    public class SystemInfo
    {
        [JsonProperty("system_id")]
        public long SystemId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}