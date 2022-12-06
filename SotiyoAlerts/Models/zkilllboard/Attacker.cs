using Newtonsoft.Json;

namespace SotiyoAlerts.Models.zkilllboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Attacker
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("alliance_id")]
        public long AllianceId { get; set; }
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
        [JsonProperty("damage_done")]
        public long DamageDone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("final_blow")]
        public bool FinalBlow { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("security_status")]
        public double SecurityStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ship_type_id")]
        public long ShipTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("weapon_type_id")]
        public long WeaponTypeId { get; set; }
    }
}
