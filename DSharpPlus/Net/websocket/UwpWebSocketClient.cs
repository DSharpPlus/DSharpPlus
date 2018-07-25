#if WINDOWS_UWP || WINDOWS_8
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Web;

namespace DSharpPlus.Net.WebSocket
{
    /// <summary>
    /// A WebSocket client based on the stock UWP <see cref="MessageWebSocket"/>
    /// </summary>
    public class WebSocketClient : BaseWebSocketClient
    {
        MessageWebSocket _rawSocket;

        public MessageWebSocket Socket => _rawSocket;

        public WebSocketClient(IWebProxy proxy) : base(proxy)
        {
            _connect = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _disconnect = new AsyncEvent<SocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _message = new AsyncEvent<SocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _error = new AsyncEvent<SocketErrorEventArgs>(null, "WS_ERROR");
        }

        public override async Task ConnectAsync(Uri uri)
        {
            _rawSocket = new MessageWebSocket();

            StreamDecompressor?.Dispose();
            CompressedStream?.Dispose();
            DecompressedStream?.Dispose();

            DecompressedStream = new MemoryStream();
            CompressedStream = new MemoryStream();
            StreamDecompressor = new DeflateStream(CompressedStream, CompressionMode.Decompress);

            if (Proxy != null) // fuck this, I ain't working with that shit either
            {
                throw new NotImplementedException("Proxies are not supported on non-stock WebSocket client implementations.");
            }

            _rawSocket.MessageReceived += _rawSocket_MessageReceived;
            _rawSocket.Closed += _rawSocket_Closed;
            await _rawSocket.ConnectAsync(uri);

            await _connect.InvokeAsync();
        }

        public override Task DisconnectAsync(SocketCloseEventArgs e)
        {
            try
            {
                _rawSocket.Close((ushort)e.CloseCode, e.CloseMessage);
            }
            catch
            {
                _rawSocket = new MessageWebSocket();
            }

            return Task.Delay(0);
        }

        private async void _rawSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            await _disconnect?.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = args.Code, CloseMessage = args.Reason });
        }

        private async void _rawSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var msg = "";
            try
            {

                using (DataReader reader = args.GetDataReader())
                {
                    if (args.MessageType == SocketMessageType.Binary)
                    {
                        byte[] data = new byte[reader.UnconsumedBufferLength];
                        reader.ReadBytes(data);

                        if (data[0] == 0x78)
                        {
                            CompressedStream.Write(data, 2, data.Length - 2);
                        }
                        else
                        {
                            CompressedStream.Write(data, 0, data.Length);
                        }

                        CompressedStream.Flush();
                        CompressedStream.Position = 0;

                        // partial credit to FiniteReality
                        // overall idea is his
                        // I tuned the finer details
                        // -Emzi
                        // I copied it over
                        // -Wam
                        var sfix = BitConverter.ToUInt16(data, data.Length - 2);
                        if (sfix != ZLIB_STREAM_SUFFIX)
                        {
                            using (var zlib = new DeflateStream(CompressedStream, CompressionMode.Decompress, true))
                            {
                                zlib.CopyTo(DecompressedStream);
                            }
                        }
                        else
                        {
                            StreamDecompressor.CopyTo(DecompressedStream);
                        }

                        msg = Encoding.UTF8.GetString(DecompressedStream.ToArray(), 0, (int)DecompressedStream.Length);

                        DecompressedStream.Position = 0;
                        DecompressedStream.SetLength(0);
                        CompressedStream.Position = 0;
                        CompressedStream.SetLength(0);
                    }
                    else
                    {
                        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        msg = reader.ReadString(reader.UnconsumedBufferLength);
                    }
                }
            }
            catch (Exception ex)
            {
                await _error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex });
            }

            if (msg != null)
            {
                await _message?.InvokeAsync(new SocketMessageEventArgs() { Message = msg });
            }
        }

        public override void SendMessage(string message)
        {
            try
            {
                using (DataWriter dataWriter = new DataWriter(_rawSocket.OutputStream))
                {
                    dataWriter.WriteString(message);
                    dataWriter.StoreAsync().AsTask().GetAwaiter().GetResult();
                    dataWriter.DetachStream();
                }
            }
            catch (Exception ex)
            {
                _error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();

                //_disconnect?.InvokeAsync(new SocketCloseEventArgs(null) { CloseCode = args.Code, CloseMessage = args.Reason });
            }
        }

        public static BaseWebSocketClient CreateNew(IWebProxy proxy)
            => new WebSocketClient(proxy);

        protected override Task OnConnectedAsync() => Task.Delay(0);

        protected override Task OnDisconnectedAsync(SocketCloseEventArgs e) => Task.Delay(0);

        #region Events
        /// <summary>
        /// Triggered when the client connects successfully.
        /// </summary>
        public override event AsyncEventHandler OnConnect
        {
            add { _connect.Register(value); }
            remove { _connect.Unregister(value); }
        }
        private AsyncEvent _connect;

        /// <summary>
        /// Triggered when the client is disconnected.
        /// </summary>
        public override event AsyncEventHandler<SocketCloseEventArgs> OnDisconnect
        {
            add { _disconnect.Register(value); }
            remove { _disconnect.Unregister(value); }
        }
        private AsyncEvent<SocketCloseEventArgs> _disconnect;

        /// <summary>
        /// Triggered when the client receives a message from the remote party.
        /// </summary>
        public override event AsyncEventHandler<SocketMessageEventArgs> OnMessage
        {
            add { _message.Register(value); }
            remove { _message.Unregister(value); }
        }
        private AsyncEvent<SocketMessageEventArgs> _message;

        /// <summary>
        /// Triggered when an error occurs in the client.
        /// </summary>
        public override event AsyncEventHandler<SocketErrorEventArgs> OnError
        {
            add { _error.Register(value); }
            remove { _error.Unregister(value); }
        }
        private AsyncEvent<SocketErrorEventArgs> _error;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
            {
                Debug.WriteLine($"WSERROR: {ex.GetType()} in {evname}!");
            }
            else
            {
                _error.InvokeAsync(new SocketErrorEventArgs(null) { Exception = ex }).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
        #endregion
    }
}
#endif