using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Caching.Memory;
using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Util;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SotiyoAlerts.Modules
{
    public class FilterComponentModule : InteractionModuleBase
    {
        private readonly IChannelFilterService _channelFilterService;
        private readonly IMemoryCache _cache;

        public FilterComponentModule(IChannelFilterService channelFilterService, IMemoryCache cache)
        {
            _cache = cache;
            _channelFilterService = channelFilterService;
        }

        [ComponentInteraction("add-select")]
        public async Task FilterSelection(string[] selections)
        {
            long channelId = Convert.ToInt64(Context.Channel.Id);
            long filterId = Convert.ToInt64(selections.FirstOrDefault());

            _cache.Set($"add_select_{channelId}", filterId);
            SelectMenuBuilder subMenu = ComponentUtil.GetSubFilterSelectMenu(filterId);

            var components = new ComponentBuilder().WithSelectMenu(subMenu);

            await Context.Interaction.RespondAsync("Please select a sub category to further filter the killmails!",
                components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("add-select-sub")]
        public async Task SubFilterSelection(string[] selections)
        {
            long channelId = Convert.ToInt64(Context.Channel.Id);
            long subFilterId = Convert.ToInt64(selections.FirstOrDefault());

            if (_cache.TryGetValue($"add_select_{channelId}", out long filterId))
            {
                var existingFilters = _channelFilterService.GetChannelFilters(channelId);

                if (existingFilters.Any(cf => cf.FilterId == filterId && cf.SubFilterId == subFilterId))
                {
                    await Context.Interaction.RespondAsync("A filter of the same type is already applied to this channel. Use `sa-info` [channel_id] to get filters already applied to a channel.",
                        ephemeral: true);
                    return;
                }

                // Prevent overlapping filters.
                switch (filterId)
                {
                    // TODO: More detailed messages about what exactly is conflicting.
                    // Also try to remove magic #s
                    case (long)Filters.SotiyoSystemKills:
                        if (subFilterId == (long)SubFilter.BothSotiyo && existingFilters
                            .Count(ef => ef.FilterId == (long)Filters.SotiyoSystemKills) == 2)
                        {
                            await Context.Interaction.RespondAsync(
                                "You are trying to add a new filter that would overlap with existing filters.",
                                ephemeral: true);
                            return;
                        }

                        if (existingFilters.Any(ef => ef.SubFilterId == (long)SubFilter.BothSotiyo))
                        {
                            await Context.Interaction.RespondAsync(
                                "You are trying to add a new filter that would overlap with existing filters.",
                                ephemeral: true);
                            return;
                        }
                        break;
                    case (long)Filters.NpcOfficerKills:
                        if (subFilterId == (long)SubFilter.AllOfficers && existingFilters
                            .Count(ef => ef.FilterId == (long)Filters.NpcOfficerKills) == 6)
                        {
                            await Context.Interaction.RespondAsync(
                                "You are trying to add a new filter that would overlap with existing filters.",
                                ephemeral: true);
                            return;
                        }

                        if (existingFilters.Any(ef => ef.SubFilterId == (long)SubFilter.AllOfficers))
                        {
                            await Context.Interaction.RespondAsync(
                                "You are trying to add a new filter that would overlap with existing filters.",
                                ephemeral: true);
                            return;
                        }
                        break;
                }
            }
            try
            {
                _channelFilterService.AddChannelFilter(channelId, filterId, subFilterId);
                await Context.Interaction.RespondAsync("Filter added succesfully! Start using the filter now using /sa-start",
                    ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync(
                    $"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
            finally
            {
                _cache.Remove($"add_select_{channelId}");
            }
        }

        [ComponentInteraction("add-select-yes")]
        public async Task ContinueWithNewTracking()
        {
            MessageComponent menu = ComponentUtil.GetFilterComponent();
            await Context.Interaction.RespondAsync("Please choose the desired filter type.", components: menu, ephemeral: true);
        }

        [ComponentInteraction("add-select-no")]
        public async Task StopWithNewTracking()
        {
            await Context.Interaction.DeleteOriginalResponseAsync();
        }

        [ComponentInteraction("start-select")]
        public async Task FilterEnable(string[] selections)
        {
            try
            {
                foreach (string filterSelect in selections)
                {
                    long channelFilterId = Convert.ToInt64(filterSelect);
                    _channelFilterService.EnableChannelFilter(channelFilterId);
                }

                await Context.Interaction.RespondAsync($"Successfully enabled {selections.Length} filter(s).", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [ComponentInteraction("stop-select")]
        public async Task FilterDisable(string[] selections)
        {
            try
            {
                foreach (string filterSelect in selections)
                {
                    long channelFilterId = Convert.ToInt64(filterSelect);
                    _channelFilterService.DisableChannelFilter(channelFilterId);
                }

                await Context.Interaction.RespondAsync($"Successfully disabled {selections.Length} filter(s).", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }

        [ComponentInteraction("delete-select")]
        public async Task FilterDelete(string[] selections)
        {
            try
            {
                foreach (string filterSelect in selections)
                {
                    long channelFilterId = Convert.ToInt64(filterSelect);
                    _channelFilterService.MarkChannelFilterDeleted(channelFilterId);
                }

                await Context.Interaction.RespondAsync($"Successfully marked {selections.Length} filter(s) as deleted.", ephemeral: true);
            }
            catch (Exception ex)
            {
                await Context.Interaction.RespondAsync($"An error occured while performing your last action: {ex.Message}", ephemeral: true);
            }
        }
    }
}
