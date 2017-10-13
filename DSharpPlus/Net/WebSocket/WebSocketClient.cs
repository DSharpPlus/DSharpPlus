using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Net.WebSocket
{
    // weebsocket
    public class WebSocketClient : BaseWebSocketClient
    {
        private const int BufferSize = 32768;

        private static UTF8Encoding Utf8 { get; set; }
        
        private ConcurrentQueue<string> SocketMessageQueue { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken Token => TokenSource.Token;

        private ClientWebSocket Socket { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private Task WsListener { get; set; }
        private Task SocketQueueManager { get; set; }

        #region Events
        public override event AsyncEventHandler OnConnect
        {
            add { _onConnect.Register(value); }
            remove { _onConnect.Unregister(value); }
        }
        private AsyncEvent _onConnect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _onDisconnect.Register(value); }
            remove { _onDisconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _onDisconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _onMessage.Register(value); }
            remove { _onMessage.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _onMessage;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _onError.Register(value); }
            remove { _onError.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _onError;
        #endregion

        public WebSocketClient()
        {
            _onConnect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _onDisconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _onMessage = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _onError = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        static WebSocketClient()
        {
            Utf8 = new UTF8Encoding(false);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <returns></returns>
        public static new WebSocketClient Create()
        {
            return new WebSocketClient();
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public override async Task<BaseWebSocketClient> ConnectAsync(string uri)
        {
            SocketMessageQueue = new ConcurrentQueue<string>();
            TokenSource = new CancellationTokenSource();

            await InternalConnectAsync(new Uri(uri));
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public override async Task<BaseWebSocketClient> OnConnectAsync()
        {
            await _onConnect.InvokeAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public override async Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e)
        {
            await _onDisconnect.InvokeAsync(e);
            return this;
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public override void SendMessage(string message)
        {
            SendMessageAsync(message);
        }

        internal void SendMessageAsync(string message)
        {
            if (Socket.State != WebSocketState.Open)
            {
                return;
            }

            SocketMessageQueue.Enqueue(message);

            if (SocketQueueManager == null || SocketQueueManager.IsCompleted)
            {
                SocketQueueManager = Task.Run(SmqTask, Token);
            }
        }

        internal async Task InternalConnectAsync(Uri uri)
        {
            Socket = new ClientWebSocket();
            Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

            await Socket.ConnectAsync(uri, Token);
            await CallOnConnectedAsync();
            WsListener = Task.Run(Listen, Token);
        }

        private bool _closeRequested;
        public override async Task InternalDisconnectAsync(SocketCloseEventArgs e)
        {
            //if (this.Socket.State != WebSocketState.Open || this.Token.IsCancellationRequested)
            if (_closeRequested)
            {
                return;
            }

            _closeRequested = true;
            try
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", Token);
                e = e ?? new SocketCloseEventArgs(null) { CloseCode = (int)WebSocketCloseStatus.NormalClosure, CloseMessage = "" };
                Socket.Abort();
                Socket.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                if (e == null)
                {
                    var cc = Socket.CloseStatus != null ? (int)Socket.CloseStatus.Value : -1;
                    e = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = Socket.CloseStatusDescription ?? "Unknown reason" };
                }
                await CallOnDisconnectedAsync(e);
            }
        }

        internal async Task Listen()
        {
            await Task.Yield();

            var buff = new byte[BufferSize];
            var buffseg = new ArraySegment<byte>(buff);

            SocketCloseEventArgs close = null;

            var token = Token;
            
            try
            {
                while (!token.IsCancellationRequested && Socket.State == WebSocketState.Open)
                {
                    byte[] resultbuff;
                    WebSocketReceiveResult result;
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await Socket.ReceiveAsync(buffseg, token);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                var cc = result.CloseStatus != null ? (int)result.CloseStatus.Value : -1;
                                close = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = result.CloseStatusDescription };
                            }
                            else
                            {
                                ms.Write(buff, 0, result.Count);
                            }
                        }
                        while (!result.EndOfMessage);

                        resultbuff = ms.ToArray();
                    }

                    if (close != null)
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        using (var ms1 = new MemoryStream(resultbuff, 2, resultbuff.Length - 2))
                        {
                            using (var ms2 = new MemoryStream())
                            {
                                using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                                {
                                    await zlib.CopyToAsync(ms2);
                                }

                                resultbuff = ms2.ToArray();
                            }
                        }
                    }
                    
                    var resultstr = Utf8.GetString(resultbuff, 0, resultbuff.Length);
                    await CallOnMessageAsync(resultstr);
                }
            }
            catch (Exception e)
            {
                close = new SocketCloseEventArgs(null) { CloseCode = -1, CloseMessage = e.Message };
            }

            await InternalDisconnectAsync(close);
        }

        internal async Task SmqTask()
        {
            await Task.Yield();

            var token = Token;
            while (!token.IsCancellationRequested && Socket.State == WebSocketState.Open)
            {
                if (SocketMessageQueue.IsEmpty)
                {
                    break;
                }

                if (!SocketMessageQueue.TryDequeue(out var message))
                {
                    break;
                }

                var buff = Utf8.GetBytes(message);
                var msgc = buff.Length / BufferSize;
                if (buff.Length % BufferSize != 0)
                {
                    msgc++;
                }

                for (var i = 0; i < msgc; i++)
                {
                    var off = BufferSize * i;
                    var cnt = Math.Min(BufferSize, buff.Length - off);

                    var lm = i == msgc - 1;
                    await Socket.SendAsync(new ArraySegment<byte>(buff, off, cnt), WebSocketMessageType.Text, lm, Token);
                }
            }
        }

        internal async Task CallOnMessageAsync(string result)
        {
            await _onMessage.InvokeAsync(new SocketMessageEventArgs() { Message = result });
        }

        internal Task CallOnDisconnectedAsync(SocketCloseEventArgs e)
        {
            //await _on_disconnect.InvokeAsync(e);
            
            // Zis is to prevent deadlocks (I hope)
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = _onDisconnect.InvokeAsync(e);

            return Task.Delay(0);
        }

        internal async Task CallOnConnectedAsync()
        {
            await _onConnect.InvokeAsync();
        }

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
            {
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            }
            else
            {
                _onError.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).GetAwaiter().GetResult();
            }
        }
    }
}
