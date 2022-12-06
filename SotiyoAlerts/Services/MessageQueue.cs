using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models;
using SotiyoAlerts.Models.Eve;
using SotiyoAlerts.Models.zkilllboard;
using SotiyoAlerts.Util;

namespace SotiyoAlerts.Services
{
    public class MessageQueue : IQueue<Killmail>
    {
        private readonly ActionBlock<Killmail> _jobs;
        private readonly IClassificationService _classificationService;
        private readonly DiscordSocketClient _discordClient;
        private readonly IMemoryCache _cache;
        private readonly IQueue<RetryMessage> _messageRetryQueue;
        private readonly IServiceScopeFactory _scopeFactory;

        public MessageQueue(DiscordSocketClient discordClient, IClassificationService classificationService,
            IMemoryCache cache, IServiceScopeFactory scopeFactory, IQueue<RetryMessage> messageRetryQueue)
        {
            var executionDataFlowBlockOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            _jobs = new ActionBlock<Killmail>(ProcessQueuedItem, executionDataFlowBlockOptions);
            _classificationService = classificationService;
            _cache = cache;
            _discordClient = discordClient;
            _scopeFactory = scopeFactory;
            _messageRetryQueue = messageRetryQueue;
        }

        public void Enqueue(Killmail item)
        {
            _jobs.Post(item);
        }

        private async Task ProcessQueuedItem(Killmail item)
        {
            try
            {
                var filter = _classificationService.GetFilterFromKillmail(item);
                if (filter == Data.Enums.Filters.None) return;

                var subFilter = _classificationService.GetSubFilterFromKillmail(filter, item);
                if (subFilter == Data.Enums.SubFilter.None) return;

                using var scope = _scopeFactory.CreateScope();
                var filterService = scope.ServiceProvider.GetService<IChannelFilterService>();
                HashSet<long> channels = filterService.GetChannelsForSubFilterId((long)subFilter, filter);

                string systemName = "N/A";
                if(_cache.TryGetValue($"name_{item.SolarSystemId}", out SystemInfo system))
                {
                    systemName = system?.Name ?? "N/A";
                }

                var embed = EmbedUtil.CreateEmbedForKillmail(item, subFilter, systemName);
                var components = ComponentUtil.GetKillmailComponent(item.KillmailId, item.SolarSystemId, item.Victim.CharacterId);

                // TODO: Upgrade below implementation to use some sort of thread safe concurrency.
                foreach (var channel in channels)
                {
                    try
                    {
                        if (_discordClient.GetChannel(Convert.ToUInt64(channel)) is not IMessageChannel msgChannel) return;
                        await msgChannel?.SendMessageAsync(embed: embed.Build(), components: components);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to send killmail ID: {killmailId} to channel ID: {channelId}, re-queueing for channel only at: {date}.",
                            item.KillmailId, channel, DateTimeOffset.Now);
                        _messageRetryQueue.Enqueue(new RetryMessage
                        {
                            ChannelId = channel,
                            Killmail = item,
                            Filter = filter,
                            SubFilter = subFilter,
                            Embed = embed,
                            Components = components
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Killmail {killmailId} processing failed, re-queueing killmail for error at: {date}", item.KillmailId, DateTimeOffset.Now);
                _jobs.Post(item);
            }
        }
    }
}