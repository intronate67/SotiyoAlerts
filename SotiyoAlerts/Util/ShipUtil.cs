using SotiyoAlerts.Enums;

namespace SotiyoAlerts.Util
{
    public static class ShipUtil
    {
        public static long GetDiamondShipGroup(long shipId)
        {
            return shipId switch
            {
                43605 or 43606 or 45055 or 45056 or 43626 or 43627 or 46055 or 46523 or 46524 or 46057 => (long)DiamondGroup.Battleship,
                46054 or 46760 or 46581 or 46056 or 43622 or 43623 or 43624 or 43625 or 43601 or 43602 or 43603 or 43604 => (long)DiamondGroup.Cruiser,
                46653 or 46654 or 45470 or 45474 => (long)DiamondGroup.Dreadnought,
                45680 or 46052 or 46053 or 44953 or 43597 or 43598 or 43599 or 43600 or 43614 or 43615 or 43616 or 43617 or 43618 or 46382 => (long)DiamondGroup.Frigate,
                45471 or 45472 => (long)DiamondGroup.Titan,
                _ => default,
            };
        }

        public static OfficerGroup GetNpcOfficerGroup(long shipId)
        {
            return shipId switch
            {
                13536 or 13538 or 13541 or 13544 => OfficerGroup.AngelCartelOfficer,
                13557 or 13561 or 13564 or 13573 => OfficerGroup.BloodRaidersOfficer,
                13580 or 13584 or 13589 or 13603 => OfficerGroup.GuristasOfficer,
                13609 or 13615 or 13622 or 13635 => OfficerGroup.SanshasNationOfficer,
                13654 or 13659 or 13661 or 13667 => OfficerGroup.SerpentisOfficer,
                32959 or 32960 or 32961 or 32962 => OfficerGroup.RogueDroneOfficer,
                _ => OfficerGroup.Invalid,
            };
        }
    }
}