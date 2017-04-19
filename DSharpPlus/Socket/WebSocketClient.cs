using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSharpPlus
{
    public class WebSocketClient : BaseWebSocketClient
    {
        internal const int ReceiveChunkSize = 1024;
        internal const int SendChunkSize = 1024;
        
        internal ClientWebSocket _ws;
        internal Uri _uri;
        internal CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        internal CancellationToken _cancellationToken;
        internal bool _connected;

        public override event AsyncEventHandler OnConnect
        {
            add { this._on_connect.Register(value); }
            remove { this._on_connect.Unregister(value); }
        }
        private AsyncEvent _on_connect = new AsyncEvent();

        public override event AsyncEventHandler OnDisconnect
        {
            add { this._on_disconnect.Register(value); }
            remove { this._on_disconnect.Unregister(value); }
        }
        private AsyncEvent _on_disconnect = new AsyncEvent();

        public override event AsyncEventHandler<WebSocketMessageEventArgs> OnMessage
        {
            add { this._on_message.Register(value); }
            remove { this._on_message.Unregister(value); }
        }
        private AsyncEvent<WebSocketMessageEventArgs> _on_message = new AsyncEvent<WebSocketMessageEventArgs>();

        protected WebSocketClient()
        {
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static new WebSocketClient Create()
        {
            return new WebSocketClient();
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public override async Task<WebSocketClient> ConnectAsync(string uri)
        {
            _uri = new Uri(uri);
            await InternalConnectAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <returns></returns>
        public override async Task<WebSocketClient> OnConnectAsync()
        {
            await _on_connect.InvokeAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <returns></returns>
        public override async Task<WebSocketClient> OnDisconnectAsync()
        {
            await _on_disconnect.InvokeAsync();
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
            if (_ws.State != WebSocketState.Open)
            {
                return;
            }

            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SendChunkSize);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SendChunkSize * i);
                var count = SendChunkSize;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }
                _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken).Wait();
            }
        }

        internal async Task InternalConnectAsync()
        {
            // laziness intensifies
            _connected = true;
            _ws = new ClientWebSocket();
            try
            {
                await _ws.ConnectAsync(_uri, _cancellationToken);
                await CallOnConnectedAsync();
                await StartListen();
            }
            catch (Exception)
            {

            }
        }
        public override async Task InternalDisconnectAsync()
        {
            // lazy again
            _connected = false;
            try
            {
                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cancellationToken).Wait();
                _ws.Abort();
                _ws.Dispose();
                await CallOnDisconnectedAsync();
            }
            catch (Exception)
            {

            }
        }

        internal async Task InternalReconnectAsync()
        {
            await InternalDisconnectAsync();
            await InternalConnectAsync();
        }

        internal async Task StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];
            try
            {
                while (_connected)
                {
                    var stringResult = new StringBuilder();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);
                        await Task.Yield();
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await InternalDisconnectAsync();
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);
                    await CallOnMessageAsync(stringResult);
                }
            }
            catch (Exception)
            {
            }
            await InternalDisconnectAsync();
        }

        internal async Task CallOnMessageAsync(StringBuilder stringResult)
        {
            await _on_message.InvokeAsync(new WebSocketMessageEventArgs()
            {
                Message = stringResult.ToString()
            });
        }

        internal async Task CallOnDisconnectedAsync()
        {
            await _on_disconnect.InvokeAsync();
        }

        internal async Task CallOnConnectedAsync()
        {
            await _on_connect.InvokeAsync();
        }
    }
}