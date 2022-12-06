using SotiyoAlerts.Data.Enums;
using SotiyoAlerts.Data.Models;
using System.Collections.Generic;

namespace SotiyoAlerts.Interfaces
{
    public interface IChannelFilterService
    {
        HashSet<long> GetChannelsForSubFilterId(long subFilterId, Filters filter);
        HashSet<long> GetFiltersForChannelId(long channelId);
        HashSet<ChannelFilter> GetChannelFilters(long channelId);
        HashSet<ChannelFilter> GetChannelFiltersForGuild(long guildId);
        void AddChannelFilter(long channelId, long filterId, long subFilterId);
        void EnableChannelFilter(ChannelFilter channelFilter);
        void EnableChannelFilter(long channelFilterId);
        void DisableChannelFilter(ChannelFilter channelFilter);
        void DisableChannelFilter(long channelFilterId);
        void MarkChannelFilterDeleted(ChannelFilter channelFilter);
        void MarkChannelFilterDeleted(long channelFilterId);
        bool CheckFilterExistence(long channelId, long filterId);
        bool CheckFilterExistence(long channelId, long filterId, long subFilterId);
    }
}
