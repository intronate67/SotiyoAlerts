using System.Collections.Generic;
using Newtonsoft.Json;
using SotiyoAlerts.Models.zkilllboard;

namespace SotiyoAlerts.Models.zkillboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Victim
    {
        /// <summary>
        /// 
        /// </summary>
        public int? AllianceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("character_id")]
        public long CharacterId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("corporation_id")]
        public long CorporationId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("damage_taken")]
        public long DamageTaken { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("position")]
        public Position Position { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ship_type_id")]
        public long ShipTypeId { get; set; }
    }
}
