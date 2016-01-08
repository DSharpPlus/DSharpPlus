using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using WebSocketSharp;

namespace DiscordSharp
{
    internal class VoiceConnectionParameters
    {
        public string ssrc { get; internal set; }
        public int port { get; internal set; }
        public string[] modes { get; internal set; }
        public int heartbeat_interval { get; internal set; }
    }

    class DiscordVoiceClient
    {
        WebSocket VoiceWebSocket;
        public string SessionID { get; internal set; }
        public string VoiceEndpoint { get; internal set; }
        public string Token { get; internal set; }
        public DiscordServer Guild { get; internal set; }
        public DiscordMember Me { get; internal set; }

        private VoiceConnectionParameters Params { get; set; }

        public void Initiate()
        {
            VoiceWebSocket = new WebSocket("wss://" + VoiceEndpoint.Replace(":80", ""));
            VoiceWebSocket.OnMessage += (sender, e) =>
            {
                JObject message = JObject.Parse(e.Data);
                if(message["op"].Value<int>() == 2)
                {
                    Params = new VoiceConnectionParameters
                    {
                        ssrc = message["d"]["ssrc"].ToString(),
                        port = message["d"]["port"].Value<int>(),
                        modes = message["d"]["modes"].Value<string[]>(),
                        heartbeat_interval = message["d"]["heartbeat_interval"].Value<int>()
                    };
                    InitialConnection();
                }
                
            };
            VoiceWebSocket.OnOpen += (sender, e) =>
            {
                string initMsg = JsonConvert.SerializeObject(new
                {
                    op = 0,
                    d = new
                    {
                        user_id = Me.user.id,
                        server_id = Guild.id,
                        session_id = SessionID,
                        token = Token
                    }
                });

                VoiceWebSocket.Send(initMsg);
            };
            VoiceWebSocket.Connect();
        }

        private void InitialConnection()
        {
            using (BinaryWriter s = new BinaryWriter(new MemoryStream()))
            {
                s.Write(Params.ssrc);
            }
        }
    }
}
