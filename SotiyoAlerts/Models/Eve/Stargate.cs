using Newtonsoft.Json;

namespace SotiyoAlerts.Models.Eve
{
    public class Stargate
    {
        [JsonProperty("stargate_id")]
        public long StargateId { get; set; }
        [JsonProperty("destination_stargate_id")]
        public long DestinationStargateId { get; set; }
        [JsonProperty("x")]
        public double X { get; set; }
        [JsonProperty("y")]
        public double Y { get; set; }
        [JsonProperty("z")]
        public double Z { get; set; }
    }
}