using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Net.WebSockets;

namespace DiscordSharp
{
    public class SocketBinaryEventArgs : EventArgs
    {
        public byte[] message { get; internal set; }
        public string type { get; internal set; }
        internal SocketBinaryEventArgs(byte[] msg)
        {
            message = msg;
        }
    }
    public class SocketTextEventArgs : EventArgs
    {
        public JObject message { get; internal set; }
        public string type { get; internal set; }
        internal SocketTextEventArgs(string msg)
        {
            message = JObject.Parse(msg);
        }
    }

    internal static class TaskHelper
    {
        public static Task CompletedTask { get; }
        static TaskHelper()
        {
            CompletedTask = Task.Delay(0);
        }
    }

    public class CustomWebSocket
    {
        public event EventHandler<SocketTextEventArgs> TextMessageReceived;
        public event EventHandler<SocketBinaryEventArgs> BinaryMessageReceived;


        private readonly ConcurrentQueue<string> _sendQueue;
        private WebSocketSharp.WebSocket _webSocket;

        private readonly DiscordClient ___parent;

        public CustomWebSocket(DiscordClient _parent)
        {
            ___parent = _parent;
            _sendQueue = new ConcurrentQueue<string>();

        }

        public Task Connect(string host, CancellationToken cancelToken)
        {
            _webSocket = new WebSocketSharp.WebSocket(host);
            _webSocket.EmitOnPing = false;
            _webSocket.EnableRedirection = true;
            _webSocket.Compression = WebSocketSharp.CompressionMethod.Deflate;
            _webSocket.OnMessage += (s, e) =>
            {
                if (e.Type == WebSocketSharp.Opcode.Binary)
                {
                    if (BinaryMessageReceived != null)
                        BinaryMessageReceived(this, new SocketBinaryEventArgs(e.RawData));
                }
                else if (e.Type == WebSocketSharp.Opcode.Text)
                {
                    if (TextMessageReceived != null)
                        TextMessageReceived(this, new SocketTextEventArgs(e.Data));
                }
            };
            _webSocket.OnError += (s, e) =>
            {

            };
            return TaskHelper.CompletedTask;
        }
    }
}
