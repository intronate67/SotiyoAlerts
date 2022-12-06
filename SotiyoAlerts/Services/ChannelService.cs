using Serilog;
using SotiyoAlerts.Data;
using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Interfaces;
using System;
using System.Linq;

namespace SotiyoAlerts.Services
{
    public class ChannelService : IChannelService
    {
        private readonly IGuildService _guildService;
        private readonly SotiyoAlertsDb _ctx;

        public ChannelService(SotiyoAlertsDb ctx, IGuildService guildService)
        {
            _ctx = ctx;
            _guildService = guildService;
        }

        public Channel GetChannel(long channelId)
        {
            return _ctx.Channels.FirstOrDefault(c => c.Id == channelId);
        }

        public void AddNewChannel(long channelId, long guildId, string name)
        {
            if (CheckChannelExistence(channelId))
            {
                throw new InvalidOperationException("Channel already exists!");
            }

            var guild = _guildService.GetGuild(guildId);

            if (guild == null)
            {
                throw new InvalidOperationException("Guild has disapeared from the database!");
            }

            Log.Information("Adding new channel {name} ({id})", name, channelId);

            var channel = Channel.Create(channelId, guildId, name, DateTime.Now, guild);

            _ctx.Channels.Add(channel);
            _ctx.SaveChanges();
        }

        public bool CheckChannelExistence(long channelId)
        {
            return _ctx.Channels.Any(g => g.Id == channelId);
        }
    }
}
