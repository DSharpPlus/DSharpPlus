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
        private const int BUFFER_SIZE = 32768;

        private static UTF8Encoding UTF8 { get; set; }
        
        private ConcurrentQueue<string> SocketMessageQueue { get; set; }
        private CancellationTokenSource TokenSource { get; set; }
        private CancellationToken Token => this.TokenSource.Token;

        private ClientWebSocket Socket { get; set; }
        private Task WsListener { get; set; }
        private Task SocketQueueManager { get; set; }

        #region Events
        public override event AsyncEventHandler OnConnect
        {
            add { this._on_connect.Register(value); }
            remove { this._on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect;

        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _on_disconnect;

        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _on_message;

        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { this._on_error.Register(value); }
            remove { this._on_error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _on_error;
        #endregion

        public WebSocketClient()
        {
            this._on_connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._on_disconnect = new AsyncEvent<SocketCloseEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._on_message = new AsyncEvent<SocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
            this._on_error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        static WebSocketClient()
        {
            UTF8 = new UTF8Encoding(false);
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
            this.SocketMessageQueue = new ConcurrentQueue<string>();
            this.TokenSource = new CancellationTokenSource();

            await InternalConnectAsync(new Uri(uri));
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public override async Task<BaseWebSocketClient> OnConnectAsync()
        {
            await _on_connect.InvokeAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public override async Task<BaseWebSocketClient> OnDisconnectAsync(SocketCloseEventArgs e)
        {
            await _on_disconnect.InvokeAsync(e);
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
                return;

            this.SocketMessageQueue.Enqueue(message);

            if (this.SocketQueueManager == null || this.SocketQueueManager.IsCompleted)
                this.SocketQueueManager = Task.Run(this.SmqTask, this.Token);
        }

        internal async Task InternalConnectAsync(Uri uri)
        {
            this.Socket = new ClientWebSocket();
            this.Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

            await Socket.ConnectAsync(uri, this.Token);
            await CallOnConnectedAsync();
            this.WsListener = Task.Run(this.Listen, this.Token);
        }

        private bool close_requested = false;
        public override async Task InternalDisconnectAsync(SocketCloseEventArgs e)
        {
            //if (this.Socket.State != WebSocketState.Open || this.Token.IsCancellationRequested)
            if (close_requested)
                return;

            close_requested = true;
            try
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this.Token);
                e = e ?? new SocketCloseEventArgs(null) { CloseCode = (int)WebSocketCloseStatus.NormalClosure, CloseMessage = "" };
                Socket.Abort();
                Socket.Dispose();
            }
            catch (Exception)
            { }
            finally
            {
                if (e == null)
                {
                    var cc = this.Socket.CloseStatus != null ? (int)this.Socket.CloseStatus.Value : -1;
                    e = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = this.Socket.CloseStatusDescription ?? "Unknown reason" };
                }
                await CallOnDisconnectedAsync(e);
            }
        }

        internal async Task Listen()
        {
            await Task.Yield();

            var buff = new byte[BUFFER_SIZE];
            var buffseg = new ArraySegment<byte>(buff);

            byte[] resultbuff = null;
            WebSocketReceiveResult result = null;
            SocketCloseEventArgs close = null;

            var token = this.Token;
            
            try
            {
                while (!token.IsCancellationRequested && this.Socket.State == WebSocketState.Open)
                {
                    using (var ms = new MemoryStream())
                    {
                        do
                        {
                            result = await this.Socket.ReceiveAsync(buffseg, token);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                var cc = result.CloseStatus != null ? (int)result.CloseStatus.Value : -1;
                                close = new SocketCloseEventArgs(null) { CloseCode = cc, CloseMessage = result.CloseStatusDescription };
                            }
                            else
                                ms.Write(buff, 0, result.Count);
                        }
                        while (!result.EndOfMessage);

                        resultbuff = ms.ToArray();
                    }

                    if (close != null)
                        break;

                    var resultstr = "";
                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        using (var ms1 = new MemoryStream(resultbuff, 2, resultbuff.Length - 2))
                        using (var ms2 = new MemoryStream())
                        {
                            using (var zlib = new DeflateStream(ms1, CompressionMode.Decompress))
                                await zlib.CopyToAsync(ms2);

                            resultbuff = ms2.ToArray();
                        }
                    }
                    
                    resultstr = UTF8.GetString(resultbuff, 0, resultbuff.Length);
                    await this.CallOnMessageAsync(resultstr);
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

            var token = this.Token;
            while (!token.IsCancellationRequested && this.Socket.State == WebSocketState.Open)
            {
                if (this.SocketMessageQueue.IsEmpty)
                    break;

                if (!this.SocketMessageQueue.TryDequeue(out var message))
                    break;

                var buff = UTF8.GetBytes(message);
                var msgc = buff.Length / BUFFER_SIZE;
                if (buff.Length % BUFFER_SIZE != 0)
                    msgc++;

                for (var i = 0; i < msgc; i++)
                {
                    var off = BUFFER_SIZE * i;
                    var cnt = Math.Min(BUFFER_SIZE, buff.Length - off);

                    var lm = i == msgc - 1;
                    await Socket.SendAsync(new ArraySegment<byte>(buff, off, cnt), WebSocketMessageType.Text, lm, this.Token);
                }
            }
        }

        internal async Task CallOnMessageAsync(string result)
        {
            await _on_message.InvokeAsync(new SocketMessageEventArgs() { Message = result });
        }

        internal Task CallOnDisconnectedAsync(SocketCloseEventArgs e)
        {
            //await _on_disconnect.InvokeAsync(e);
            
            // Zis is to prevent deadlocks (I hope)
            _ = this._on_disconnect.InvokeAsync(e);

            return Task.Delay(0);
        }

        internal async Task CallOnConnectedAsync()
        {
            await _on_connect.InvokeAsync();
        }

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            else
                this._on_error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).GetAwaiter().GetResult();
        }
    }
}