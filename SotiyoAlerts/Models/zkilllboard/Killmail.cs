using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SotiyoAlerts.Models.zkillboard;

namespace SotiyoAlerts.Models.zkilllboard
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Killmail
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("attackers")]
        public List<Attacker> Attackers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("killmail_id")]
        public long KillmailId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("killmail_time")]
        public DateTimeOffset KillmailTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("solar_system_id")]
        public long SolarSystemId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("victim")]
        public Victim Victim { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("zkb")]
        public Zkb Zkb { get; set; }
    }
}
