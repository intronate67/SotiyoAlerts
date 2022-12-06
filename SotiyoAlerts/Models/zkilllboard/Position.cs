using Newtonsoft.Json;

namespace SotiyoAlerts.Models.zkilllboard
{

    /// <summary>
    /// 
    /// </summary>
    public partial class Position
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("x")]
        public double X { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("y")]
        public double Y { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("z")]
        public double Z { get; set; }
    }
}
