using System;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Serilog;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models.zkillboard;
using SotiyoAlerts.Models.zkilllboard;

namespace SotiyoAlerts.Services
{
    public class DeserializationQueue : IDeserializationQueue
    {
        private readonly IQueue<Killmail> _messageQueue;
        private readonly ActionBlock<RawSocketResponse> _jobs;

        public DeserializationQueue(IQueue<Killmail> messageQueue)
        {
            var executionDataFlowBlockOptions = new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount, 
            };

            _jobs = new ActionBlock<RawSocketResponse>(ProcessQueuedItem, executionDataFlowBlockOptions);

            _messageQueue = messageQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(RawSocketResponse item) => _jobs.Post(item);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private void ProcessQueuedItem(RawSocketResponse item)
        {
            // probably should log this
            if (string.IsNullOrEmpty(item.Json)) return;

            try
            {
                Log.Information("Processing new killmail at: {date}", DateTimeOffset.Now);
                var km = JsonConvert.DeserializeObject<Killmail>(item.Json);

                if (km == null) return;

                _messageQueue.Enqueue(km);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error occurred while deserializing killmail: {date}", DateTimeOffset.Now);
                _jobs.Post(item);
            }
        }
    }
}