using Discord;
using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Models.zkilllboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SotiyoAlerts.Models
{
    public class RetryMessage
    {
        public Killmail Killmail { get; set; }
        public long ChannelId { get; set; }
        public Filters Filter { get; set; }
        public SubFilter SubFilter { get; set; }
        public EmbedBuilder Embed { get; set; }
        public MessageComponent Components { get; set; }
    }
}
