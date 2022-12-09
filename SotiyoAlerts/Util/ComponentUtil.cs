using Discord;
using SotiyoAlerts.Data.Enums;

namespace SotiyoAlerts.Util
{
    public static class ComponentUtil
    {
        public static MessageComponent GetNewTrackingComponent()
        {
            return new ComponentBuilder()
                        .WithButton("Yes", "add-select-yes", style: ButtonStyle.Success, row: 0)
                        .WithButton("No", "add-select-no", style: ButtonStyle.Danger, row: 0)
                        .Build();
        }

        public static MessageComponent GetFilterComponent()
        {
            var menu = new SelectMenuBuilder()
            {
                CustomId = "add-select",
                Placeholder = "Select Filter",
                MaxValues = 1,
                MinValues = 1
            };

            menu.AddOption("Sotiyo Tracking", "1", "Track either Diamond Blood Raiders or Guristas killmails on stargates.")
                .AddOption("NPC Officer Tracking", "2", "Track NPC Officers that appear on killmails");

            return new ComponentBuilder()
                .WithSelectMenu(menu)
                .Build();
        }

        public static SelectMenuBuilder GetSubFilterSelectMenu(long filterId)
        {
            var subMenu = new SelectMenuBuilder()
            {
                CustomId = "add-select-sub",
                Placeholder = "Select sub filter",
                MinValues = 1,
                MaxValues = 1
            };

            switch (filterId)
            {
                case (long)Filters.SotiyoSystemKills:
                    subMenu.AddOption("Blood Raiders", "1", "Blood Raiders only sotiyo system tracking.")
                        .AddOption("Guristas", "2", "Guristas only sotiyo system tracking.")
                        .AddOption("Both", "3", "Track both Guristas & Blood Raiders killmails in sotiyo systems in 1 channel");
                    break;
                case (long)Filters.NpcOfficerKills:
                    subMenu.AddOption("Blood Raiders", "4", "Blood Raiders only Officer tracking.")
                        .AddOption("Guristas", "5", "Guristas only Officer tracking.")
                        .AddOption("Sansha's Nation", "6", "Guristas only Officer tracking.")
                        .AddOption("Serpentis", "7", "Guristas only Officer tracking.")
                        .AddOption("Drones", "8", "Guristas only Officer tracking.")
                        .AddOption("All", "9", "All NPC Corporation Officer tracking.");
                    break;
            }

            return subMenu;
        }

        public static MessageComponent GetKillmailComponent(long killmailId, long systemId, long characterId)
        {
            return new ComponentBuilder()
                    .WithButton("Killmail", url: $"https://zkillboard.com/kill/{killmailId}",
                        style: ButtonStyle.Link)
                    .WithButton("System", url: $"https://zkillboard.com/system/{systemId}",
                        style: ButtonStyle.Link)
                    .WithButton("Victim", url: $"https://zkillboard.com/character/{characterId}",
                        style: ButtonStyle.Link).Build();
        }
    }
}
