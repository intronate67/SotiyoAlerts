using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models;
using SotiyoAlerts.Models.zkilllboard;
using SotiyoAlerts.Services;

namespace SotiyoAlerts
{
    public class Program
    {
        public static Task Main() => MainAsync();

        public static async Task MainAsync()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            if (IsDebug()) configBuilder.AddUserSecrets<Program>();

            var config = configBuilder.Build();
            var esUri = new Uri(config["ElasticUrl"]);
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithExceptionDetails()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(esUri)
                {
                    DetectElasticsearchVersion = true,
                    AutoRegisterTemplate = true,
                    IndexFormat = $"sotiyoalerts-{environment.ToLower()}-{{0:yyyy-MM.dd}}",
                    DeadLetterIndexName = $"sotiyoalerts-{environment.ToLower()}-{{0:yyyy-MM.dd}}",
                    RegisterTemplateFailure = RegisterTemplateRecovery.IndexAnyway
                })
                .ReadFrom.Configuration(config)
                .CreateLogger();

            Log.Information("Created logger with elasticsearch connection: {url}", esUri.OriginalString);

            var discordClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                ConnectionTimeout = int.MaxValue,
                UseInteractionSnowflakeDate = true,
                GatewayIntents = GatewayIntents.AllUnprivileged
            });

            var services = new ServiceCollection()
                .AddDbContext<Data.SotiyoAlertsDb>(opts =>
                {
                    opts.UseSqlServer(config.GetConnectionString("SotiyoAlertsDb"),
                    x => x.MigrationsAssembly("SotiyoAlerts.Data"));
                })
                .AddMemoryCache()
                .AddTransient<IClassificationService, ClassificationService>()
                .AddTransient<IConfiguration>(_ => config)
                .AddScoped<IFilterService, FilterService>()
                .AddScoped<IGuildService, GuildService>()
                .AddScoped<IChannelService, ChannelService>()
                .AddScoped<IChannelFilterService, ChannelFilterService>()
                .AddSingleton(discordClient)
                .AddSingleton<InteractionService>()
                .AddSingleton<IQueue<RetryMessage>, MessageRetryQueue>()
                .AddSingleton<IQueue<Killmail>, MessageQueue>()
                .AddSingleton<IDeserializationQueue, DeserializationQueue>()
                .AddSingleton<IZKillboardListener, ZKillboardListener>()
                .AddSingleton<BotManager>()
                .BuildServiceProvider();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var cts = new CancellationTokenSource();
            var botMgr = services.GetService<BotManager>();
            await botMgr.StartAsync(cts.Token);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.ExceptionObject as Exception, $"Unhandled exception!");
            Environment.Exit(System.Runtime.InteropServices.Marshal.GetHRForException(e.ExceptionObject as Exception));
        }

        public static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}