using Discord;
using Discord.Interactions;
using System.Linq;
using System.Threading.Tasks;

namespace SotiyoAlerts.Modules
{
    public class HelpModule : InteractionModuleBase
    {
        [SlashCommand("sa-help", "Sotiyo Alerts information"), RequireContext(ContextType.Guild)]
        public async Task Help(string command = null)
        {
            if (string.IsNullOrEmpty(command))
            {
                var helpEmbed = new EmbedBuilder()
                                .WithTitle("Sotiyo Alerts Command Help")
                                .WithAuthor(Context.Client.CurrentUser)
                                .WithDescription("List of all available commands to interact with the SotiyoAlerts bot.")
                                .AddField("sa-help", "This command. List every command available provided by this bot.", false)
                                .AddField("sa-help <command_name>", "Get more verbose help information about the specified.", false)
                                .AddField("sa-add", "Add a new filter.", false)
                                .AddField("sa-start", "Start (enable) a filter.", false)
                                .AddField("sa-stop", "Stop (disable) an active filter", false)
                                .AddField("sa-delete", "Delete a filter", false)
                                .AddField("sa-info", "List filters for the current channel.", false)
                                .AddField("sa-info <channel_id>", "List filters for the specified channel.", false)
                                .WithCurrentTimestamp()
                                .WithColor(Color.Blue)
                                .Build();
                await Context.Interaction.RespondAsync(embed: helpEmbed, ephemeral: true);
            }
            else
            {
                var commands = await Context.Guild.GetApplicationCommandsAsync();

                IApplicationCommand socketCommand = commands.FirstOrDefault(c => c.Name == command.ToLower());

                if (socketCommand == default)
                {
                    await Context.Interaction.RespondAsync($"The command '{command}' is not a valid command for this bot!");
                    return;
                }

                var commandEmbed = new EmbedBuilder()
                    .WithTitle($"Sotiyo Alerts '{socketCommand.Name}' command Help")
                    .WithAuthor(Context.Client.CurrentUser)
                    .WithDescription(socketCommand.Description)
                    .WithCurrentTimestamp()
                    .WithColor(Color.Blue)
                    .Build();

                await Context.Interaction.RespondAsync(embed: commandEmbed, ephemeral: true);
            }
        }
    }
}
