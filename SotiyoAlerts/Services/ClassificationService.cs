using Microsoft.Extensions.Caching.Memory;
using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Enums;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models.Eve;
using SotiyoAlerts.Models.zkilllboard;
using SotiyoAlerts.Util;
using System;
using System.Linq;

namespace SotiyoAlerts.Services
{
    public class ClassificationService : IClassificationService
    {
        // This value is completely arbitrary but generally values around than the minimum
        // allowed structure distance (from stargate) have been promising.
        private const double MaxNpcStargateDistance = 150000d;

        private readonly IMemoryCache _cache;

        public ClassificationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Filters GetFilterFromKillmail(Killmail killmail)
        {
            // This implementation has a pitfall which is that it will 
            // only ever classify a killmail with one type of filter
            // and more specifically, the first one it matches, the rest
            // will be ignored so may want to add support for a killmail 
            // to be classified with multiple filters.
            if (IsSotiyoSystemKill(killmail)) return Filters.SotiyoSystemKills;
            if (IsNpcOfficerKill(killmail)) return Filters.NpcOfficerKills;

            return Filters.None;
        }

        public SubFilter GetSubFilterFromKillmail(Filters filter, Killmail killmail)
        {
            switch (filter)
            {
                case Filters.SotiyoSystemKills:
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Guristas))
                        return SubFilter.GuristasSotiyo;
                    return SubFilter.BloodRaidersSotiyo;
                case Filters.NpcOfficerKills:
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.BloodRaiders))
                        return SubFilter.BloodRaiderOfficer;
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Guristas))
                        return SubFilter.GuristasOfficer;
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Angel))
                        return SubFilter.AngelOfficer;
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Sansha))
                        return SubFilter.SansahsOfficer;
                    if (killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Serpentis))
                        return SubFilter.SerpentisOfficer;
                    return SubFilter.DronesOfficer;
            }

            return SubFilter.None;
        }

        private bool IsSotiyoSystemKill(Killmail killmail)
        {
            if (!_cache.TryGetValue($"security_{killmail.SolarSystemId}",
                out SecuritySystem system)) return false;

            if (system?.SecurityStatus > 0.5) return false;

            if (killmail.Attackers.All(att => ShipUtil.GetDiamondShipGroup(att.ShipTypeId) == default))
            {
                return false;
            }

            if (!DetermineStargateProximity(killmail.Victim.Position, killmail.SolarSystemId)) return false;

            return killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.BloodRaiders)
                || killmail.Attackers.Any(a => a.CorporationId == (long)NpcCorporation.Guristas);
        }

        private bool IsNpcOfficerKill(Killmail killmail)
        {
            if (!_cache.TryGetValue($"security_{killmail.SolarSystemId}",
                out SecuritySystem system)) return false;

            if (system?.SecurityStatus > 0.5) return false;

            if (killmail.Attackers.All(att => ShipUtil.GetNpcOfficerGroup(att.ShipTypeId) == OfficerGroup.Invalid))
            {
                return false;
            }

            return true;
        }

        private bool DetermineStargateProximity(Position pos, long systemId)
        {
            if (!_cache.TryGetValue($"stargate_{systemId}", out StargateInfo system)) return false;
            return system?.Stargates.Any(s => IsWithin(s.X, s.Y, s.Z, pos, MaxNpcStargateDistance)) ?? false;
        }

        private static bool IsWithin(Position pos1, Position pos2, double distance)
        {
            var dx = Math.Abs(pos1.X - pos2.X);
            var dy = Math.Abs(pos1.Y - pos2.Y);
            var dz = Math.Abs(pos1.Z - pos2.Z);
            return Math.Sqrt(dx * dx + dy * dy + dz * dz) <= distance;
        }

        private static bool IsWithin(double x, double y, double z, Position pos2, double distance)
        {
            return IsWithin(new Position
            {
                X = x,
                Y = y,
                Z = z
            }, pos2, distance);
        }
    }
}
