using Newtonsoft.Json;

namespace SotiyoAlerts.Models.Eve
{
    public class SecuritySystem
    {
        [JsonProperty("system_id")]
        public long SystemId { get; set; }

        [JsonProperty("security_status")]
        public double SecurityStatus { get; set; }
    }
}