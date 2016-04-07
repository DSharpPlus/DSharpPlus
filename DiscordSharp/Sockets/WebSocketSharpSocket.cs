using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace DiscordSharp.Sockets
{
    class WebSocketSharpSocket : IDiscordWebSocket
    {
        private WebSocket Socket;

        public string URL { get; set; }
        public bool IsAlive
        {
            get
            {
                return Socket != null ? Socket.IsAlive : false;
            }
        }

        public event EventHandler<SocketMessageEventArgs> MessageReceived;
        public event EventHandler<SocketClosedEventArgs> SocketClosed;
        public event EventHandler<EventArgs> SocketOpened;
        public event EventHandler<SocketErrorEventArgs> SocketError;

        public WebSocketSharpSocket(string url)
        {
            URL = url;

            Socket = new WebSocket(url);

            HookupEvents();
        }

        /// <summary>
        /// Hooks up events to automatically be redirected to the interface's events.
        /// </summary>
        private void HookupEvents()
        {
            Socket.OnMessage += (sender, e) =>
            {
                SocketMessageEventArgs args = new SocketMessageEventArgs
                {
                    Message = e.Data
                };
                MessageReceived?.Invoke(this, args);
            };

            Socket.OnError += (sender, e) => 
            {
                SocketErrorEventArgs args = new SocketErrorEventArgs
                {
                    Exception = e.Exception,
                    Message = e.Message
                };
                SocketError?.Invoke(this, args);
            };

            Socket.OnClose += (sender, e) =>
            {
                SocketClosedEventArgs args = new SocketClosedEventArgs
                {
                    Code = e.Code,
                    Reason = e.Reason,
                    WasClean = e.WasClean
                };
                SocketClosed?.Invoke(this, args);
            };

            Socket.OnOpen += (sender, e) =>
            {
                SocketOpened?.Invoke(this, null);
            };
        }

        public void Connect()
        {
            Socket.Connect();
        }

        public void Close()
        {
            Socket.Close();
        }

        public void Send(string data)
        {
            Socket.Send(data);
        }

        public void Send(byte[] data)
        {
            Socket.Send(data);
        }
    }
}
