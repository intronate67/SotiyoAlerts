using Newtonsoft.Json;

namespace SotiyoAlerts.Models.zkilllboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Item
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("flag")]
        public long Flag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("item_type_id")]
        public long ItemTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("quantity_destroyed")]
        public long QuantityDestroyed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("singleton")]
        public long Singleton { get; set; }
    }
}
