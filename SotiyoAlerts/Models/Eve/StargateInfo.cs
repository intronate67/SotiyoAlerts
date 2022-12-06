using System.Collections.Generic;
using Newtonsoft.Json;

namespace SotiyoAlerts.Models.Eve
{
    public class StargateInfo
    {
        [JsonProperty("system_id")]
        public long SystemId { get; set; }
        [JsonProperty("stargates")]
        public List<Stargate> Stargates { get; set; }
    }
}