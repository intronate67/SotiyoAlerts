using Discord;
using Discord.Interactions;
using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SotiyoAlerts.Modules
{
    public class FilterModule : InteractionModuleBase
    {
        private readonly IChannelFilterService _channelFilterService;

        public FilterModule(IChannelFilterService channelFilterService)
        {
            _channelFilterService = channelFilterService;
        }

        [SlashCommand("sa-add", "Add a new tracking filter for a specified type."), RequireContext(ContextType.Guild)]
        public async Task AddTracking()
        {
            try
            {
                if (_channelFilterService.GetFiltersForChannelId(Convert.ToInt64(Context.Channel.Id)).Any())
                {
                    MessageComponent component = ComponentUtil.GetNewTrackingComponent();

                    await Context.Interaction.RespondAsync(
                        "There are already filters applied in this channel, are you sure you want to add more?",
                        components: component, ephemeral: true);
                    return;
                }

                // Add the current channel and type to the database to be used when filtering killmails
                await Context.Interaction.RespondAsync("Please choose the desired filter type.", 
                    components: ComponentUtil.GetFilterComponent(), ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("sa-start", "Start (enable) tracking for a specified type."), RequireContext(ContextType.Guild)]
        public async Task StartTracking()
        {
            var filters = _channelFilterService.GetChannelFilters(Convert.ToInt64(Context.Channel.Id));

            if (filters == null || filters?.Count == 0)
            {
                await Context.Interaction.RespondAsync("No filters were found for the current channel.", ephemeral: true);
                return;
            }

            if (filters.All(cf => cf.IsActive))
            {
                await Context.Interaction.RespondAsync("There are no existing filters to enable, try adding a new one using /sa-new",
                    ephemeral: true);
                return;
            }

            var filtersToEnable = filters.Where(cf => !cf.IsActive).ToHashSet();

            if (filtersToEnable.Count > 1)
            {
                var options = filtersToEnable
                    .Select(f => new SelectMenuOptionBuilder()
                        .WithLabel(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                        .WithValue(f.Id.ToString()))
                    .ToList();

                var selectMenu = new SelectMenuBuilder()
                {
                    CustomId = "start-select",
                    Placeholder = "Select the filter(s) to enable:",
                    MinValues = 1,
                    MaxValues = 8 // TODO: This should be updated with the total filter count, as added.
                }.WithOptions(options);

                var components = new ComponentBuilder().WithSelectMenu(selectMenu);

                await Context.Interaction.RespondAsync(components: components.Build(), ephemeral: true);
                return;
            }

            var filterToEnable = filtersToEnable.FirstOrDefault();

            try
            {
                _channelFilterService.EnableChannelFilter(filterToEnable);
                await Context.Interaction.RespondAsync("Successfully enabled the filter!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("sa-stop", 
            "Disable tracking the specified type. Does not delete the filter and it can be re-enabled at any time"),
            RequireContext(ContextType.Guild)]
        public async Task StopTracking()
        {
            var filters = _channelFilterService.GetChannelFilters(Convert.ToInt64(Context.Channel.Id));

            if (filters == null || filters?.Count == 0)
            {
                await Context.Interaction.RespondAsync("No filters were found for the current channel.", ephemeral: true);
                return;
            }

            if (filters.All(cf => !cf.IsActive))
            {
                await Context.Interaction.RespondAsync("There are no existing filters to disable.", ephemeral: true);
                return;
            }

            var filtersToDisable = filters.Where(cf => cf.IsActive).ToHashSet();

            if (filtersToDisable.Count > 1)
            {
                var options = filtersToDisable
                    .Select(f => new SelectMenuOptionBuilder()
                        .WithLabel(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                        .WithValue(f.Id.ToString()))
                    .ToList();

                var selectMenu = new SelectMenuBuilder()
                {
                    CustomId = "stop-select",
                    Placeholder = "Select the filter(s) to disable:",
                    MinValues = 1,
                    MaxValues = 8 // TODO: This should be updated with the total filter count, as added.
                }.WithOptions(options);

                var components = new ComponentBuilder().WithSelectMenu(selectMenu);

                await Context.Interaction.RespondAsync(components: components.Build(), ephemeral: true);
                return;
            }

            var filterToDisable = filtersToDisable.FirstOrDefault();
            try
            {
                _channelFilterService.DisableChannelFilter(filterToDisable);
                await Context.Interaction.RespondAsync("Successfully enabled the filter!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("sa-delete", "Delete killmail filter for a specified type."), RequireContext(ContextType.Guild)]
        public async Task DeleteTracking()
        {
            var filters = _channelFilterService.GetChannelFilters(Convert.ToInt64(Context.Channel.Id));

            if (filters == null || filters?.Count == 0)
            {
                await Context.Interaction.RespondAsync("No filters were found for the current channel.", ephemeral: true);
                return;
            }

            var filtersToDelete = filters.Where(cf => !cf.IsActive).ToHashSet();

            if (filtersToDelete.Count > 1)
            {
                var options = filtersToDelete
                    .Select(f => new SelectMenuOptionBuilder()
                        .WithLabel(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                        .WithValue(f.Id.ToString()))
                    .ToList();

                var selectMenu = new SelectMenuBuilder()
                {
                    CustomId = "delete-select",
                    Placeholder = "Select the filter(s) to delete:",
                    MinValues = 1,
                    MaxValues = 8 // TODO: This should be updated with the total filter count, as added.
                }.WithOptions(options);

                var components = new ComponentBuilder().WithSelectMenu(selectMenu);

                await Context.Interaction.RespondAsync(components: components.Build(), ephemeral: true);
                return;
            }

            var filterToDelete = filtersToDelete.FirstOrDefault();
            try
            {
                _channelFilterService.MarkChannelFilterDeleted(filterToDelete);
                await Context.Interaction.RespondAsync("Successfully enabled the filter!", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [SlashCommand("sa-info", "List all the active/inactive filters for a channel."), RequireContext(ContextType.Guild)]
        public async Task GetChannelFilter([ChannelTypes(ChannelType.Text), Summary(description:
            "(Optional) Specify a channel other than the current channel")] IChannel channel = null)
        {
            if (channel == null)
            {
                var filters = _channelFilterService.GetChannelFilters(Convert.ToInt64(Context.Channel.Id));

                if (filters == null || filters?.Count == 0)
                {
                    await Context.Interaction.RespondAsync("No filters were found for the current channel.", ephemeral: true);
                    return;
                }

                var filterFields = filters.Select(f => new EmbedFieldBuilder()
                        .WithName(Enum.GetName(typeof(Filters), f.FilterId))
                        .WithValue(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                        .WithIsInline(true));
                var filterEmbed = new EmbedBuilder()
                    .WithTitle($"{Context.Channel.Name} Channel Info")
                    .WithAuthor(Context.User)
                    .WithDescription("List of all killmail filters that have ever been created for this channel")
                    .WithFields(filterFields)
                    .WithCurrentTimestamp()
                    .WithColor(Color.Blue)
                    .Build();
                await Context.Interaction.RespondAsync($"Found {filterEmbed.Fields.Length} existing filter(s) for this channel.",
                    embed: filterEmbed, ephemeral: true);
            }
            else
            {
                var filters = _channelFilterService.GetChannelFilters(Convert.ToInt64(channel.Id));

                if (filters == null || filters?.Count == 0)
                {
                    await Context.Interaction.RespondAsync("No filters were found for the specified channel.", ephemeral: true);
                    return;
                }

                var filterFields = filters.Select(f => new EmbedFieldBuilder()
                        .WithName(Enum.GetName(typeof(Filters), f.FilterId))
                        .WithValue(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                        .WithIsInline(true));
                var filterEmbed = new EmbedBuilder()
                    .WithTitle($"{channel.Name} Channel Info")
                    .WithAuthor(Context.User)
                    .WithDescription("List of all killmail filters that have ever been created for the specified channel")
                    .WithFields(filterFields)
                    .WithCurrentTimestamp()
                    .WithColor(Color.Blue)
                    .Build();
                await Context.Interaction.RespondAsync($"Found {filters.Count} existing filter(s) for the specified channel.",
                    embed: filterEmbed, ephemeral: true);
            }
        }

        [SlashCommand("sa-info-all", "List all the active/inactive filters for the entire guild."), RequireContext(ContextType.Guild)]
        public async Task GetChannelFilter()
        {
            var filters = _channelFilterService.GetChannelFiltersForGuild(Convert.ToInt64(Context.Guild.Id));

            if (filters == null || filters?.Count == 0)
            {
                await Context.Interaction.RespondAsync("No filters were found for this guild.", ephemeral: true);
                return;
            }

            var filterFields = filters.Select(f => new EmbedFieldBuilder()
                    .WithName(Enum.GetName(typeof(Filters), f.FilterId))
                    .WithValue(Enum.GetName(typeof(SubFilter), f.SubFilterId))
                    .WithIsInline(true));
            var filterEmbed = new EmbedBuilder()
                .WithTitle("Sotiyo Alerts Guild Info")
                .WithAuthor(Context.User)
                .WithDescription("List of all killmail filters that have ever been created for this guild")
                .WithFields(filterFields)
                .WithCurrentTimestamp()
                .WithColor(Color.Blue)
                .Build();
            await Context.Interaction.RespondAsync($"Found {filterEmbed.Fields.Length} existing filter(s) for this guild.",
                embed: filterEmbed, ephemeral: true);
        }
    }
}
