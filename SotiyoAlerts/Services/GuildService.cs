using Serilog;
using SotiyoAlerts.Data;
using SotiyoAlerts.Data.Models;
using SotiyoAlerts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SotiyoAlerts.Services
{
    public class GuildService : IGuildService
    {
        private readonly SotiyoAlertsDb _ctx;

        public GuildService(SotiyoAlertsDb ctx)
        {
            _ctx = ctx;
        }

        public HashSet<Guild> GetGuilds() => _ctx.Guilds?.ToHashSet();
        public HashSet<long> GetAllGuilds() => _ctx.Guilds.Select(g => g.Id)?.ToHashSet();
        public Guild GetGuild(long guildId) => _ctx.Guilds.FirstOrDefault(g => g.Id == guildId);

        public void AddNewGuild(long guildId, string name) => AddNewGuild(guildId, name, false);

        public void AddNewGuild(long guildId, string name, bool deleted)
        {
            if (CheckGuildExistence(guildId))
            {
                throw new InvalidOperationException("Guild already exists!");
            }

            Log.Information("Adding new Guild {name} ({id})", name, guildId);

            var guild = Guild.Create(guildId, name, true, deleted, DateTime.Now);

            _ctx.Guilds.Add(guild);
            _ctx.SaveChanges();
        }

        public void MarkGuildDeleted(long guildId, bool deleted)
        {
            var guild = GetGuild(guildId);
            if (deleted)
            {
                guild.IsDeleted = true;
                guild.IsActive = false;
            }
            else
            {
                guild.IsDeleted = false;
                guild.IsActive = true;
            }

            guild.ModifiedTime = DateTime.Now;
            _ctx.SaveChanges();
        }

        public bool CheckGuildExistence(long guildId) => _ctx.Guilds.Any(g => g.Id == guildId);
        
    }
}
