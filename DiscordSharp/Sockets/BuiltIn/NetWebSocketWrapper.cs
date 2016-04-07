using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordSharp.Sockets.BuiltIn
{
    public class NetWebSocketWrapper
    {
        private const int ReceiveChunkSize = 1024;
        private const int SendChunkSize = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Action<NetWebSocketWrapper> _onConnected;
        private Action<string, NetWebSocketWrapper> _onMessage;

        /// <summary>
        /// CloseStatus.Value.ToString()
        /// CloseStatusDescription
        /// Socket
        /// </summary>
        private Action<int, string, NetWebSocketWrapper> _onDisconnected;

        protected NetWebSocketWrapper(string uri)
        {
            _ws = new ClientWebSocket();
            _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;

            //_ws.CloseStatusDescription
            //_ws.CloseStatus.Value.ToString();
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static NetWebSocketWrapper Create(string uri)
        {
            return new NetWebSocketWrapper(uri);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public NetWebSocketWrapper Connect()
        {
            ConnectAsync();
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <param name="onConnect">The Action to call.</param>
        /// <returns></returns>
        public NetWebSocketWrapper OnConnect(Action<NetWebSocketWrapper> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <param name="onDisconnect">The Action to call</param>
        /// <returns></returns>
        public NetWebSocketWrapper OnDisconnect(Action<int, string, NetWebSocketWrapper> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when a messages has been received.
        /// </summary>
        /// <param name="onMessage">The Action to call.</param>
        /// <returns></returns>
        public NetWebSocketWrapper OnMessage(Action<string, NetWebSocketWrapper> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(string message)
        {
            SendMessageAsync(message);
        }

        public void Close()
        {
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "User requested to exit.", _cancellationToken);
        }

        private void SendMessageAsync(string message)
        {
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
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

                /*await*/ _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken).Wait();
            }
        }

        private void ConnectAsync()
        {
            //await _ws.ConnectAsync(_uri, _cancellationToken);
            _ws.ConnectAsync(_uri, _cancellationToken).Wait();
            CallOnConnected();
            StartListen();
        }

        private void StartListen()
        {
            var buffer = new byte[ReceiveChunkSize];

            try
            {
                while (_ws.State == WebSocketState.Open)
                {
                    var stringResult = new StringBuilder();


                    WebSocketReceiveResult result;
                    do
                    {
                        result = _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken).Result;
                            //await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            /*
                            await
                                _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                                */
                            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
                            CallOnDisconnected(null);
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    CallOnMessage(stringResult);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                CallOnDisconnected(ex.Message);
            }
            finally
            {
                _ws.Dispose();
            }
        }

        private void CallOnMessage(StringBuilder stringResult)
        {
            if (_onMessage != null)
                _onMessage(stringResult.ToString(), this);
                //RunInTask(() => _onMessage(stringResult.ToString(), this));
        }

        private void CallOnDisconnected(string messageOverride)
        {
                _onDisconnected?.Invoke(_ws.CloseStatus != null ? (int)_ws.CloseStatus.Value : -1, 
                    messageOverride != null ? messageOverride : _ws.CloseStatusDescription, 
                this);
                //RunInTask(() => _onDisconnected((int)_ws.CloseStatus.Value, _ws.CloseStatusDescription, this));
        }

        private void CallOnConnected()
        {
            if (_onConnected != null)
                _onConnected(this);
                //RunInTask(() => _onConnected(this));
        }

        //private static void RunInTask(Action action)
        //{
        //    Task.Factory.StartNew(action);
        //}
    }
}
