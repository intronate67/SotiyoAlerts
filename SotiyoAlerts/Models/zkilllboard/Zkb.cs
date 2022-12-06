using System;
using Newtonsoft.Json;

namespace SotiyoAlerts.Models.zkillboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Zkb
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("locationID")]
        public long LocationId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("hash")]
        public string Hash { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("fittedValue")]
        public double FittedValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("totalValue")]
        public double TotalValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("points")]
        public long Points { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("npc")]
        public bool Npc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("solo")]
        public bool Solo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("awox")]
        public bool Awox { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("esi")]
        public Uri Esi { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}
