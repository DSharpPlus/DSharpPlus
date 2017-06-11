using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus
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

        public override event AsyncEventHandler<SocketDisconnectEventArgs> OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketDisconnectEventArgs> _on_disconnect;

        public override event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<WebSocketMessageEventArgs> _on_message;
        #endregion

        public WebSocketClient()
        {
            this._on_connect = new AsyncEvent(this.EventErrorHandler, "WS_CONNECT");
            this._on_disconnect = new AsyncEvent<SocketDisconnectEventArgs>(this.EventErrorHandler, "WS_DISCONNECT");
            this._on_message = new AsyncEvent<WebSocketMessageEventArgs>(this.EventErrorHandler, "WS_MESSAGE");
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
        public override async Task<BaseWebSocketClient> OnDisconnectAsync(SocketDisconnectEventArgs e)
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
            try
            {
                this.Socket = new ClientWebSocket();
                this.Socket.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);

                await Socket.ConnectAsync(uri, this.Token);
                await CallOnConnectedAsync();
                this.WsListener = Task.Run(this.Listen, this.Token);
            }
            catch (Exception) { }
        }

        private bool close_requested = false;
        public override async Task InternalDisconnectAsync(SocketDisconnectEventArgs e)
        {
            //if (this.Socket.State != WebSocketState.Open || this.Token.IsCancellationRequested)
            if (close_requested)
                return;

            close_requested = true;
            try
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", this.Token);
                e = e ?? new SocketDisconnectEventArgs(null) { CloseCode = (int)WebSocketCloseStatus.NormalClosure, CloseMessage = "" };
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
                    e = new SocketDisconnectEventArgs(null) { CloseCode = cc, CloseMessage = this.Socket.CloseStatusDescription ?? "Unknown reason" };
                }
                await CallOnDisconnectedAsync(e);
            }
        }

        internal async Task Listen()
        {
            await Task.Yield();

            var buff = new byte[BUFFER_SIZE];
            var buffseg = new ArraySegment<byte>(buff);
            var rsb = new StringBuilder();
            var result = (WebSocketReceiveResult)null;
            var close = (SocketDisconnectEventArgs)null;

            var token = this.Token;
            
            try
            {
                while (!token.IsCancellationRequested && this.Socket.State == WebSocketState.Open)
                {
                    do
                    {
                        result = await this.Socket.ReceiveAsync(buffseg, token);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            var cc = result.CloseStatus != null ? (int)result.CloseStatus.Value : -1;
                            close = new SocketDisconnectEventArgs(null) { CloseCode = cc, CloseMessage = result.CloseStatusDescription };
                        }
                        else
                            rsb.Append(UTF8.GetString(buff, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    if (close != null)
                        break;

                    await this.CallOnMessageAsync(rsb.ToString());
                    rsb.Clear();
                }
            }
            catch (Exception e)
            {
                close = new SocketDisconnectEventArgs(null) { CloseCode = -1, CloseMessage = e.Message };
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
            await _on_message.InvokeAsync(new WebSocketMessageEventArgs() { Message = result });
        }

        internal Task CallOnDisconnectedAsync(SocketDisconnectEventArgs e)
        {
            //await _on_disconnect.InvokeAsync(e);

#pragma warning disable 4014
            // Zis is to prevent deadlocks (I hope)
            this._on_disconnect.InvokeAsync(e);
#pragma warning restore 4014

            return Task.Delay(0);
        }

        internal async Task CallOnConnectedAsync()
        {
            await _on_connect.InvokeAsync();
        }

        private void EventErrorHandler(string evname, Exception ex)
        {
            Console.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
        }
    }
}