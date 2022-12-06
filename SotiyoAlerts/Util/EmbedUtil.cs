using Discord;
using OpenGraphNet;
using OpenGraphNet.Metadata;
using SotiyoAlerts.Enums;
using SotiyoAlerts.Models.zkilllboard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SotiyoAlerts.Util
{
    public class EmbedUtil
    {
        public static EmbedBuilder CreateEmbedForKillmail(Killmail killmail, Data.Enums.SubFilter subFilter,
            string systemName)
        {
            var npcCorporation = GetCorporation(killmail.Attackers, subFilter);
            var (title, description) = GetOpenGraphMeta(killmail.Zkb.Url.ToString());

            EmbedBuilder builder = new();
            switch (subFilter)
            {
                case Data.Enums.SubFilter.BothSotiyo:
                case Data.Enums.SubFilter.GuristasSotiyo:
                case Data.Enums.SubFilter.BloodRaidersSotiyo:
                    builder.WithColor(Color.Blue)
                        .WithThumbnailUrl($"https://images.evetech.net/corporations/{(long)npcCorporation}/logo")
                        .WithAuthor($"\u2666 {npcCorporation} Activity", url: $"https://images.evetech.net/types/{killmail.Victim.ShipTypeId}/icon")
                        .WithTitle(title)
                        .WithDescription(description)
                        .AddField("System:", systemName, true)
                        .AddField("Killmail Time:", killmail.KillmailTime, true)
                        .WithCurrentTimestamp()
                        .Build();
                    break;
                case Data.Enums.SubFilter.AllOfficers:
                case Data.Enums.SubFilter.BloodRaiderOfficer:
                case Data.Enums.SubFilter.GuristasOfficer:
                case Data.Enums.SubFilter.AngelOfficer:
                case Data.Enums.SubFilter.SansahsOfficer:
                case Data.Enums.SubFilter.SerpentisOfficer:
                case Data.Enums.SubFilter.DronesOfficer:
                    builder.WithColor(Color.Blue)
                        .WithThumbnailUrl($"https://images.evetech.net/corporations/{(long)npcCorporation}/logo")
                        .WithAuthor($"{npcCorporation} Officer Activity", url: $"https://images.evetech.net/types/{killmail.Victim.ShipTypeId}/icon")
                        .WithTitle(title)
                        .WithDescription(description)
                        .AddField("System:", systemName, true)
                        .AddField("Killmail Time:", killmail.KillmailTime, true)
                        .WithCurrentTimestamp()
                        .Build();
                    break;
            }

            return builder;
        }

        private static (string title, string description) GetOpenGraphMeta(string url)
        {
            var og = OpenGraph.ParseUrl(url, "Discord Bot", true);
            if (og.Metadata["og:url"].Value().Equals(url))
            {
                return (og.Metadata["og:title"].Value(), og.Metadata["description"].Value());
            }

            throw new ApplicationException("Unable to deserialize Open Graph metadata from zKillboard.");
        }

        private static NpcCorporation GetCorporation(List<Attacker> attackers, Data.Enums.SubFilter subFilter)
        {
            if (subFilter == Data.Enums.SubFilter.BothSotiyo)
            {
                return attackers.Any(a => a.CorporationId == (long)NpcCorporation.BloodRaiders)
                        ? NpcCorporation.BloodRaiders : NpcCorporation.Guristas;
            }

            if (subFilter == Data.Enums.SubFilter.AllOfficers)
            {
                return attackers.Any(a => a.CorporationId == (long)NpcCorporation.BloodRaiders)
                        ? NpcCorporation.BloodRaiders : attackers
                            .Any(a => a.CorporationId == (long)NpcCorporation.Guristas)
                        ? NpcCorporation.Guristas : attackers
                            .Any(a => a.CorporationId == (long)NpcCorporation.Angel)
                        ? NpcCorporation.Angel : attackers
                            .Any(a => a.CorporationId == (long)NpcCorporation.Sansha)
                        ? NpcCorporation.Sansha : attackers
                            .Any(a => a.CorporationId == (long)NpcCorporation.Serpentis)
                        ? NpcCorporation.Serpentis : NpcCorporation.Drones;
            }

            return subFilter switch
            {
                Data.Enums.SubFilter.BloodRaidersSotiyo
                    or Data.Enums.SubFilter.BloodRaiderOfficer => NpcCorporation.BloodRaiders,
                Data.Enums.SubFilter.GuristasSotiyo or Data.Enums.SubFilter.GuristasOfficer => NpcCorporation.Guristas,
                Data.Enums.SubFilter.SansahsOfficer => NpcCorporation.Sansha,
                Data.Enums.SubFilter.SerpentisOfficer => NpcCorporation.Serpentis,
                Data.Enums.SubFilter.DronesOfficer => NpcCorporation.Drones,
                Data.Enums.SubFilter.AngelOfficer => NpcCorporation.Angel,
                _ => NpcCorporation.Invalid,
            };
        }
    }
}
