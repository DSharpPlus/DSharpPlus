using DiscordSharp.Sockets.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSharp.Sockets
{
    class NetWebSocket : IDiscordWebSocket
    {
        private NetWebSocketWrapper Socket;

        public bool IsAlive
        {
            get
            {
                return Socket != null;
            }
        }

        public string URL { get; set; }

        public event EventHandler<SocketMessageEventArgs> MessageReceived;
        public event EventHandler<SocketClosedEventArgs> SocketClosed;
        public event EventHandler<SocketErrorEventArgs> SocketError;
        public event EventHandler<EventArgs> SocketOpened;

        public NetWebSocket(string url)
        {
            URL = url;
            Socket = NetWebSocketWrapper.Create(url);
            SetupEvents();
        }

        private void SetupEvents()
        {
            Socket.OnMessage((s, e) =>
            {
                SocketMessageEventArgs args = new SocketMessageEventArgs
                {
                    Message = s
                };
                MessageReceived?.Invoke(this, args);
            });
            Socket.OnConnect((s) =>
            {
                SocketOpened?.Invoke(this, null);
            });
            Socket.OnDisconnect((status, reason, socket) =>
            {
                SocketClosedEventArgs args = new SocketClosedEventArgs
                {
                    Reason = reason,
                    WasClean = false,
                    Code = status
                };
                Console.WriteLine(status);
                SocketClosed?.Invoke(this, args);
            });
        }

        public void Close()
        {
            Socket.Close();
        }

        public void Connect()
        {
            Socket.Connect();
        }

        public void Send(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Send(string data)
        {
            Socket.SendMessage(data);
        }
    }
}
