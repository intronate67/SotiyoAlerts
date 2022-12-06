using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SotiyoAlerts.Interfaces;
using SotiyoAlerts.Models.zkillboard;

namespace SotiyoAlerts.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ZKillboardListener : IZKillboardListener
    {
        /// <summary>
        /// 
        /// </summary>
        private const int BufferSize = 1024;

        /// <summary>
        /// Time, in seconds, to wait before retrying connection to zKillboard websocket.
        /// </summary>
        private const int RetryDelay = 10;

        /// <summary>
        /// Maximum amount of times to retry connection before elongating the retry delay.
        /// </summary>
        private const int MaxRetryCount = 12;

        private readonly IDeserializationQueue _deserializationQueue;

        /// <summary>
        /// 
        /// </summary>
        private ClientWebSocket _webSocket;

        public ZKillboardListener(IDeserializationQueue deserializationQueue)
        {
            _deserializationQueue = deserializationQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ConnectAsync(CancellationToken ct = default)
        {
            await ResetSocketAsync(ct);

            Log.Information("Connecting to ZKillboard Websocket...");

            try
            {
                //var socketUrl = new Uri(_configuration["ZKillSocketURL"]);
                const string socketUrl = "wss://zkillboard.com/websocket/";
                await _webSocket.ConnectAsync(new Uri(socketUrl), ct);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error was thrown while attempting to connect to zkillboard websocket.");
                throw;
            }

            Log.Information("Connected to socket at: {date}", DateTimeOffset.Now);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task StartListeningAsync(CancellationToken ct = default)
        {
            try
            {
                await SubscribeAsync(ct);
                await ReceiveAsync(ct);
            }
            catch (Exception)
            {
                Log.Information(
                    "An un-expected error occurred while listening to zKillboard Websocket at: {date}",
                    DateTimeOffset.Now);
            }
            finally
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    Log.Information("Closing ZKillboard WebSocket connection...");
                    await _webSocket.CloseAsync(WebSocketCloseStatus.Empty, "", ct);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task SubscribeAsync(CancellationToken ct = default)
        {
            var bytesToSend =
                Encoding.UTF8.GetBytes("{\r\n    \"action\":\"sub\",\r\n    \"channel\":\"killstream\"\r\n}");
            var sendBuffer = new ArraySegment<byte>(bytesToSend);

            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, ct);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReceiveAsync(CancellationToken ct = default)
        {
            var buffer = WebSocket.CreateClientBuffer(BufferSize, BufferSize);

            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var jsonResult = string.Empty;

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _webSocket.ReceiveAsync(buffer, ct);
                        jsonResult += Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>(), 0, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed", ct);
                        break;
                    }

                    if (string.IsNullOrEmpty(jsonResult)) continue;

                    //Log.Information("Queueing new killmail for processing at: {0}", DateTimeOffset.Now);
                    _deserializationQueue.Enqueue(new RawSocketResponse
                    {
                        Json = jsonResult
                    });
                }
            }
            catch (WebSocketException e)
            {
                Log.Error(e, 
                    "Connection to zKillboard Websocket lost at: {date}, waiting {delayTime}s before trying again.",
                    DateTimeOffset.Now, RetryDelay / 1000);

                // Socket will retry connection on network availability.
                if (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                {
                    if (!ct.IsCancellationRequested)
                    {
                        Log.Information(
                            "zKillboard socket connection closed, waiting for network availability.");

                        bool connected = false;
                        int count = 1;
                        while (!connected)
                        {
                            try
                            {
                                await ConnectAsync(ct);
                                connected = true;
                            }
                            catch (Exception)
                            {
                                await Task.Delay(count > MaxRetryCount ? TimeSpan.FromMinutes(5) 
                                    : TimeSpan.FromSeconds(RetryDelay), ct);
                                count++;
                            }
                        }

                        Log.Information("Re-connected to zKillboard websocket!");
                        await StartListeningAsync(ct);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ResetSocketAsync(CancellationToken ct = default)
        {
            Log.Information("Resetting ZKillboard WebSocket...");

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed", ct);
            }

            // Abort() disposes the object as well as making sure it is in the Aborted state then we instantiate
            // a new one. Though this isn't ideal, it's really the only option with an aborted ClientWebSocket.
            _webSocket?.Abort();
            _webSocket = new ClientWebSocket();
        }
    }
}
