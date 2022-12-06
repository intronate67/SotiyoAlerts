using SotiyoAlerts.Data.Models;

namespace SotiyoAlerts.Interfaces
{
    public interface IChannelService
    {
        Channel GetChannel(long channelId);
        void AddNewChannel(long channelId, long guildId, string name);
        bool CheckChannelExistence(long channelId);
    }
}
