using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Serilog;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models.Eve; 

namespace SotiyoAlerts.Services
{
    internal sealed class BotManager 
    {
        private const int DiscordSocketBuffer = 5000;

        private int _exitCode = 0;

        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly IZKillboardListener _zKillboardListener;

        private readonly DiscordSocketClient _discordClient;
        private readonly InteractionService _interactionService;

        public BotManager(IConfiguration configuration, IMemoryCache cache, IZKillboardListener zKillboardListener,
            IServiceProvider serviceProvider, DiscordSocketClient discordClient, InteractionService interactionService)
        {
            _cache = cache;
            _configuration = configuration;
            _discordClient = discordClient;
            _interactionService = interactionService;
            _serviceProvider = serviceProvider;
            _zKillboardListener = zKillboardListener;

            _discordClient.JoinedGuild += DiscordBot_JoinedGuild;
            _discordClient.LeftGuild += DiscordBot_LeftGuild;
            _discordClient.Log += DiscordBot_Log;
            _discordClient.Ready += DiscordBot_Ready;
            _discordClient.InteractionCreated += DiscordBot_InteractionCreated;
            _interactionService.SlashCommandExecuted += DiscordBot_SlashCommandExecuted;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Should probably add crash reporting to these catches.
            try
            {
                Log.Information("Setting up database...");
                SetupDatabase();

                Log.Information("Loading static data...");
                LoadStaticData(cancellationToken);

                Log.Information("Logging in to Discord...");
                string token = _configuration.GetValue<string>("DiscordToken");
                await _discordClient.LoginAsync(TokenType.Bot, token);

                Log.Information("Logged in to Discord! Starting Discord Bot Client...");
                await _discordClient.StartAsync();
                await Task.Delay(DiscordSocketBuffer, cancellationToken).ConfigureAwait(false);

                Log.Information("Discord Bot Client started!");

                await _zKillboardListener.ConnectAsync(cancellationToken);

                Log.Information("Finished starting up! Starting to listen...");
                // This call is blocking
                await _zKillboardListener.StartListeningAsync(cancellationToken);
            }
            catch (FileNotFoundException fnex)
            {
                Log.Error(fnex, "One or more json resources could not be found.");
                _exitCode = fnex.HResult;
            }
            catch (TaskCanceledException tex)
            {
                Log.Error(tex, "Task was cancelled.");
                _exitCode = tex.HResult;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected exception thrown!");
                _exitCode = ex.HResult;
            }
            finally
            {
                Log.Information("Logging out...");
                await _discordClient.LogoutAsync();
                Log.Information("Logged out. Stopping Discord Bot Client...");
                await _discordClient.StopAsync();
                await StopAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Debug("Exiting with return code: {exitCode}", _exitCode);
            _cache.Dispose();

            Log.CloseAndFlush();
            // Exit code may be null if the user cancelled via Ctrl+C/SIGTERM
            Environment.Exit(_exitCode);
            return Task.CompletedTask;
        }

        private Task DiscordBot_JoinedGuild(SocketGuild arg)
        {
            _ = Task.Factory.StartNew(() =>
            {
                var guildId = Convert.ToInt64(arg.Id);

                Log.Information("Joined guild! ID: {id} | Name: {name}", guildId, arg.Name);

                using var scope = _serviceProvider.CreateScope();
                var guildService = scope.ServiceProvider.GetService<IGuildService>();
                if (guildService.CheckGuildExistence(guildId)) guildService.MarkGuildDeleted(guildId, false);
                else guildService.AddNewGuild(guildId, arg.Name);
            }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        private Task DiscordBot_LeftGuild(SocketGuild arg)
        {
            _ = Task.Factory.StartNew(() =>
            {
                var guildId = Convert.ToInt64(arg.Id);

                Log.Information("Left guild! ID: {id} | Name: {name}", guildId, arg.Name);

                using var scope = _serviceProvider.CreateScope();
                var guildService = scope.ServiceProvider.GetService<IGuildService>();
                if (guildService.CheckGuildExistence(guildId)) guildService.MarkGuildDeleted(guildId, true);
                else guildService.AddNewGuild(guildId, arg.Name, true);
            }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        private Task DiscordBot_Ready()
        {
            _ = Task.Factory.StartNew(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                Log.Information("Adding modules...");
                await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), scope.ServiceProvider);

                if (_discordClient.Guilds.Count > 0)
                {
                    Log.Information("Checking for missing guilds in the database...");

                    var guildService = scope.ServiceProvider.GetRequiredService<IGuildService>();
                    HashSet<ulong> dbGuilds = guildService.GetAllGuilds()?.Select(g => Convert.ToUInt64(g))?.ToHashSet();

                    HashSet<SocketGuild> missingGuilds = _discordClient.Guilds
                        .ExceptBy(dbGuilds, (sg) => sg.Id)
                        ?.ToHashSet();
                    if (missingGuilds?.Count > 0)
                    {
                        Log.Information("Found {count} guilds that are missing from the database.",
                            missingGuilds.Count);
                        foreach (var guild in missingGuilds)
                        {
                            Log.Information("Adding missing Guild: {name} ({id}) to the database.",
                                guild.Id, guild.Name);
                            guildService.AddNewGuild(Convert.ToInt64(guild.Id), guild.Name);
                        }
                    }
                }
                Log.Information("Database up to date with currently joined guilds.");

                try
                {
                    if (Program.IsDebug())
                    {
                        Log.Information("Registering commands on development server...");
                        await _interactionService
                            .RegisterCommandsToGuildAsync(_configuration.GetValue<ulong>("TestGuildId"));
                    }
                    else
                    {
                        Log.Information("Registering commands globally...");
                        await _interactionService.RegisterCommandsGloballyAsync(true);
                    }

                }
                catch (HttpException ex)
                {
                    string errorJson = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
                    Log.Error(ex, errorJson);
                    _exitCode = ex.HResult;
                }
            }, TaskCreationOptions.LongRunning);

            return Task.CompletedTask;
        }

        private Task DiscordBot_InteractionCreated(SocketInteraction interaction)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    Log.Information(
                        "Interaction ({type}) created by: {username} ({userId}) in Channel: {channelName} ({channelId}) on Guild: {guildId}",
                        interaction.Type ,interaction.User.Username, interaction.User.Id, interaction.Channel.Name, interaction.ChannelId,
                        interaction.GuildId);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var channelService = scope.ServiceProvider.GetService<IChannelService>();
                        var channelId = Convert.ToInt64(interaction.Channel.Id);
                        if (!channelService.CheckChannelExistence(channelId))
                        {
                            Log.Information("Adding new Channel (Name: {name} | ID: {id}) to the database.",
                                interaction.Channel.Name, interaction.GuildId);
                            channelService.AddNewChannel(channelId, Convert.ToInt64(interaction.GuildId), interaction.Channel.Name);
                        }
                    }

                    var context = new SocketInteractionContext(_discordClient, interaction);
                    await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Exception thrown while adding new channel or executing command");
                    if (interaction.Type is InteractionType.ApplicationCommand)
                    {
                        await interaction.DeleteOriginalResponseAsync();
                    }
                }
            }, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        private Task DiscordBot_SlashCommandExecuted(SlashCommandInfo slashInfo, IInteractionContext ctx, IResult result)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                // Return if command executed successfully.
                if (result.IsSuccess)
                {
                    Log.Information("/{command} command run by: {user} ({userId}) completed successfully.",
                        slashInfo.Name, ctx.User.Username, ctx.User.Id);
                    return;
                }
                Log.Warning("/{command} command run by: {user} ({userId}) failed! Reason: {reason}.",
                        slashInfo.Name, ctx.User.Username, ctx.User.Id, result.ErrorReason);
                // TODO: Handle errors 
                switch (result.Error)
                {
                    case InteractionCommandError.BadArgs:
                    case InteractionCommandError.ConvertFailed:
                    case InteractionCommandError.Exception:
                    case InteractionCommandError.ParseFailed:
                    case InteractionCommandError.UnmetPrecondition:
                    case InteractionCommandError.UnknownCommand:
                    case InteractionCommandError.Unsuccessful:
                    default:
                        await ctx.Interaction.RespondAsync($"An error occured :( Reason: {result.ErrorReason}", ephemeral: true);
                        break;
                }
            }, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        private Task DiscordBot_Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Error:
                    Log.Error(message.Exception, message.Message);
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.Message);
                    break;
                case LogSeverity.Info:
                    Log.Information(message.Message);
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message.Message);
                    break;
                case LogSeverity.Critical:
                    Log.Fatal(message.Message);
                    break;
                case LogSeverity.Debug:
                default:
                    Log.Debug(message.Message);
                    break;
            }
            return Task.CompletedTask;
        }

        private void SetupDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetService<Data.SotiyoAlertsDb>();
            ctx.Database.Migrate();

            Data.Models.Filter[] filters = GetFiltersToSeed();
            Data.Models.SubFilter[] subFilters = GetSubFiltersToSeed(filters);

            var existingSubFilters = ctx.SubFilters.ToHashSet();

            if (existingSubFilters.Count == subFilters.Length)
            {
                Log.Information("SubFilters (and consequently, Filters) already seeded.");
            }
            else
            {
                var existingFilters = ctx.Filters.ToHashSet();
                if (existingFilters.Count == filters.Length)
                {
                    Log.Information("Filters already seeded.");
                }
                else
                {
                    Log.Information("Seeding Filters...");
                    ctx.Filters.AddRange(filters);
                }

                Log.Information("Seeding SubFilters...");
                ctx.SubFilters.AddRange(subFilters);

                ctx.SaveChanges();
            }

            Log.Information("Created & Seeded the database successfully!");
        }

        private static Data.Models.Filter[] GetFiltersToSeed()
        {
            return new Data.Models.Filter[2]
            {
                Data.Models.Filter.Create("SotiyoSystemKills", DateTime.Now),
                Data.Models.Filter.Create("NpcOfficerKills", DateTime.Now)
            };
        }

        private static Data.Models.SubFilter[] GetSubFiltersToSeed(Data.Models.Filter[] filters)
        {
            return new Data.Models.SubFilter[10]
            {
                Data.Models.SubFilter.Create(1, "BloodRaidersSotiyo", DateTime.Now, filters[0]),
                Data.Models.SubFilter.Create(1, "GuristasSotiyo", DateTime.Now, filters[0]),
                Data.Models.SubFilter.Create(1, "BothSotiyo", DateTime.Now, filters[0]),
                Data.Models.SubFilter.Create(2, "BloodRaiderOfficer", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "GuristasOfficer", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "SansahsOfficer", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "SerpentisOfficer", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "DronesOfficer", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "AllOfficers", DateTime.Now, filters[1]),
                Data.Models.SubFilter.Create(2, "AngelOfficer", DateTime.Now, filters[1])
            };
        }

        private void LoadStaticData(CancellationToken cancellationToken)
        {
            try
            {
                string systemNamesJsonFilePath = Path.Combine(Environment.CurrentDirectory,
                    "Resources", "system_names.json");
                string inSystemJson = File.ReadAllText(systemNamesJsonFilePath);
                List<SystemInfo> systemNames = JsonConvert.DeserializeObject<List<SystemInfo>>(inSystemJson);

                systemNames.ForEach(sys => _cache.Set($"name_{sys.SystemId}", sys,
                    new CancellationChangeToken(cancellationToken)));

                string systemJsonFilePath = Path.Combine(Environment.CurrentDirectory,
                    "Resources", "systems.json");
                string inJson = File.ReadAllText(systemJsonFilePath);
                List<SecuritySystem> systemSecurities = JsonConvert.DeserializeObject<List<SecuritySystem>>(inJson);

                systemSecurities.ForEach(sys => _cache.Set($"security_{sys.SystemId}", sys,
                    new CancellationChangeToken(cancellationToken)));

                string stargateJsonFilePath = Path.Combine(Environment.CurrentDirectory,
                    "Resources", "stargate_locations.json");
                string inStargateJson = File.ReadAllText(stargateJsonFilePath);
                List<StargateInfo> systemStargates = JsonConvert.DeserializeObject<List<StargateInfo>>(inStargateJson);

                systemStargates.ForEach(sys => _cache.Set($"stargate_{sys.SystemId}", sys,
                    new CancellationChangeToken(cancellationToken)));
            }
            catch (Exception)
            {
                throw new ApplicationException("Unable to load static data for FilterService");
            }
        }
    }
}