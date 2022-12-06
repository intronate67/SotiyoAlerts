using SotiyoAlerts.Data.Models;
using System.Collections.Generic;

namespace SotiyoAlerts.Interfaces
{
    public interface IGuildService
    {
        HashSet<Guild> GetGuilds();
        HashSet<long> GetAllGuilds();
        Guild GetGuild(long guildId);
        void AddNewGuild(long guildId, string name);
        void AddNewGuild(long guildId, string name, bool deleted);
        void MarkGuildDeleted(long guildId, bool deleted);
        bool CheckGuildExistence(long guildId);
    }
}
