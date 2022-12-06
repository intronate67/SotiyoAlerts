using Discord;
using Discord.WebSocket;
using Serilog;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SotiyoAlerts.Services
{
    public class MessageRetryQueue : IQueue<RetryMessage>
    {
        private const int MaxRetryCount = 3;

        private readonly DiscordSocketClient _discordClient;
        private readonly ActionBlock<RetryMessage> _jobs;
        private readonly Dictionary<Group, int> attempts;

        public MessageRetryQueue(DiscordSocketClient discordClient)
        {
            var executionDataFlowBlockOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            };

            _jobs = new ActionBlock<RetryMessage>(ProcessQueuedItem, executionDataFlowBlockOptions);
            _discordClient = discordClient;
        }


        public void Enqueue(RetryMessage item)
        {
            _jobs.Post(item);
        }

        private async Task ProcessQueuedItem(RetryMessage item)
        {
            var group = new Group { ChannelId = item.ChannelId, KillmailId = item.Killmail.KillmailId };
            if(attempts.TryGetValue(group, out int attemptCount))
            {
                if (attemptCount >= MaxRetryCount)
                {
                    Log.Warning("Killmail ID: {killmailId} failed to send to channel: {channelId} after {retryCount} attempts, removing from retry queue at: {date}", item.Killmail.KillmailId, item.ChannelId, MaxRetryCount, DateTimeOffset.Now);
                    attempts.Remove(group);
                    return;
                }
                else attempts[group] += 1;
            }
            else attempts.Add(group, 1);
            
            try
            {
                if (_discordClient.GetChannel(Convert.ToUInt64(item.ChannelId)) is not IMessageChannel msgChannel) return;
                await msgChannel?.SendMessageAsync(embed: item.Embed.Build(), components: item.Components);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed attempt to retry killmail message for Killmail: {killmailId} | ChannelID: {channelId} | Attempt Count: {attemptCount}, re-queueing at: {date}", item.Killmail.KillmailId, item.ChannelId, attemptCount == 0 ? 1 : attemptCount, DateTimeOffset.Now);
                _jobs.Post(item);
            }
        }

        private class Group
        {
            public long KillmailId { get; set; }
            public long ChannelId { get; set; }
        }
    }
}
