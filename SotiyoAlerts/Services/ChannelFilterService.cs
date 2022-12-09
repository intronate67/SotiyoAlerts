using Serilog;
using SotiyoAlerts.Data;
using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;

namespace SotiyoAlerts.Services
{
    public class ChannelFilterService : IChannelFilterService
    {
        private readonly SotiyoAlertsDb _ctx;
        private readonly IChannelService _channelService;
        private readonly IFilterService _filterService;
        public ChannelFilterService(SotiyoAlertsDb ctx, IChannelService channelService, IFilterService filterService)
        {
            _ctx = ctx;
            _channelService = channelService;
            _filterService = filterService;
        }

        public HashSet<ChannelFilter> GetChannelFilters(long channelId)
        {
            return _ctx.ChannelFilters.Where(cf => cf.ChannelId == channelId && !cf.IsDeleted)?.ToHashSet();
        }

        public HashSet<ChannelFilter> GetChannelFiltersForGuild(long guildId)
        {
            return (from cf in _ctx.ChannelFilters
                    join c in _ctx.Channels on cf.ChannelId equals c.Id
                    where c.GuildId == guildId && !cf.IsDeleted
                    select cf)?.ToHashSet();
        }

        public HashSet<long> GetChannelsForSubFilterId(long subFilterId, Filters filter)
        {
            long combinedFilterId = default;
            switch (filter)
            {
                case Filters.SotiyoSystemKills:
                    combinedFilterId = (long)Data.Enums.SubFilter.BothSotiyo;
                    break;
                case Filters.NpcOfficerKills:
                    combinedFilterId = (long)Data.Enums.SubFilter.AllOfficers;
                    break;

            }
            return _ctx.ChannelFilters
                .Where(cf => (cf.SubFilterId == subFilterId || cf.SubFilterId == combinedFilterId) && !cf.IsDeleted)
                .Select(cf => cf.ChannelId)?.ToHashSet();
        }
        
        public HashSet<long> GetFiltersForChannelId(long channelId)
        {
            return _ctx.ChannelFilters.Where(cf => cf.ChannelId == channelId && !cf.IsDeleted)
                .Select(cf => cf.FilterId)?.ToHashSet();
        }

        public void AddChannelFilter(long channelId, long filterId, long subFilterId)
        {
            if(CheckFilterExistence(channelId, filterId))
            {
                throw new InvalidOperationException("ChannelFilter already exists!");
            }

            var channel = _channelService.GetChannel(channelId);

            if (channel == null)
            {
                throw new InvalidOperationException("Channel has disapeared from the database!");
            }

            var filter = _filterService.GetFilter(filterId);

            if (filter == null)
            {
                throw new InvalidOperationException("Filter has disapeared from the database!");
            }

            var deletedChannelFilter = GetChannelFilter(channelId, filterId, subFilterId);

            if(deletedChannelFilter != null)
            {
                Log.Information(
                    "Re-adding (Un-deleting) ChannelFilter (Channel: {channelId} | Filter: {filterId})",
                    channelId, filterId);

                deletedChannelFilter.IsDeleted = false;
                deletedChannelFilter.ModifiedTime = DateTime.Now;
                _ctx.ChannelFilters.Update(deletedChannelFilter);
            }
            else
            {
                Log.Information("Adding new ChannelFilter (Channel: {channelId} | Filter: {filterId})",
                    channelId, filterId);

                var channelFilter = ChannelFilter.Create(filterId, subFilterId, channelId, false, false, DateTime.Now,
                    filter, channel);

                _ctx.ChannelFilters.Add(channelFilter);
            }
            
            _ctx.SaveChanges();
        }

        public void EnableChannelFilter(ChannelFilter channelFilter)
        {
            if (channelFilter.IsDeleted)
            {
                throw new ApplicationException("Channel filter must be re-added before it can be enabled!");
            }

            channelFilter.IsActive = true;
            channelFilter.ModifiedTime = DateTime.Now;

            _ctx.ChannelFilters.Update(channelFilter);
            _ctx.SaveChanges();
        }
        public void EnableChannelFilter(long channelFilterId)
        {
            var channelFilter = GetChannelFilter(channelFilterId);

            if (channelFilter == null) throw new ApplicationException("ChannelFilter not found!");

            EnableChannelFilter(channelFilter);
        }

        public void DisableChannelFilter(ChannelFilter channelFilter)
        {
            if (channelFilter.IsDeleted)
            {
                throw new ApplicationException("Channel filter must be re-added before it can be disabled!");
            }

            channelFilter.IsActive = false;
            channelFilter.ModifiedTime = DateTime.Now;

            _ctx.ChannelFilters.Update(channelFilter);
            _ctx.SaveChanges();
        }

        public void DisableChannelFilter(long channelFilterId)
        {
            var channelFilter = GetChannelFilter(channelFilterId);

            if (channelFilter == null) throw new ApplicationException("ChannelFilter not found!");

            DisableChannelFilter(channelFilter);
        }

        public void MarkChannelFilterDeleted(ChannelFilter channelFilter)
        {
            if (channelFilter.IsDeleted)
            {
                throw new ApplicationException("Channel filter must be re-added before it can be deleted!");
            }

            channelFilter.IsDeleted = true;
            channelFilter.ModifiedTime = DateTime.Now;

            _ctx.ChannelFilters.Update(channelFilter);
            _ctx.SaveChanges();
        }

        public void MarkChannelFilterDeleted(long channelFilterId)
        {
            var channelFilter = GetChannelFilter(channelFilterId);

            if (channelFilter == null) throw new ApplicationException("ChannelFilter not found!");

            MarkChannelFilterDeleted(channelFilter);
        }

        public bool CheckFilterExistence(long channelId, long filterId)
        {
            return _ctx.ChannelFilters
                .Any(cf => cf.ChannelId == channelId && cf.FilterId == filterId && !cf.IsDeleted);
        }

        public bool CheckFilterExistence(long channelId, long filterId, long subFilterId)
        {
            return _ctx.ChannelFilters
                .Any(cf => cf.ChannelId == channelId && cf.FilterId == filterId 
                    && cf.SubFilterId == subFilterId && !cf.IsDeleted);
        }

        /// <summary>
        /// Get a channel filter regardless of its deleted state
        /// </summary>
        /// <param name="channelFilterId"></param>
        /// <returns></returns>
        private ChannelFilter GetChannelFilter(long channelFilterId)
        {
            return _ctx.ChannelFilters.FirstOrDefault(cf => cf.Id == channelFilterId);
        }

        /// <summary>
        /// Get a channel filter regardless of its deleted state
        /// </summary>
        /// <param name="channelFilterId"></param>
        /// <returns></returns>
        private ChannelFilter GetChannelFilter(long channelId, long filterId, long subFilterId)
        {
            return _ctx.ChannelFilters
                .FirstOrDefault(cf => cf.ChannelId == channelId && cf.FilterId == filterId
                    && cf.SubFilterId == subFilterId);
        }
    }
}
