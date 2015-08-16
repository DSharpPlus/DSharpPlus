using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DiscordGUI
{
    public partial class View : Form
    {
        public View()
        {
            InitializeComponent();
            if (Program.MainDiscordClient != null)
            {
                Program.MainDiscordClient.ChannelCreated += MainDiscordClient_ChannelCreated;
                Program.MainDiscordClient.MessageReceived += MainDiscordClient_MessageReceived;
                Program.MainDiscordClient.PrivateMessageReceived += MainDiscordClient_PrivateMessageReceived;
                Program.MainDiscordClient.Connected += (senderr, ee) =>
                {
                    MessageBox.Show("Welcome, " + ee.username);
                };
                Thread t = new Thread(Program.MainDiscordClient.ConnectAndReadMessages);
                t.Start();
            }
        }

        private void hookups() {
            
        }
        private void MainDiscordClient_PrivateMessageReceived(object sender, DiscordSharp.DiscordPrivateMessageEventArgs e)
        {
            richTextBox1.Text += String.Format("[-Private Message received from {0}: {1}\n",
                e.author.user.username,
                e.message);
        }

        private void MainDiscordClient_MessageReceived(object sender, DiscordSharp.Events.DiscordMessageEventArgs e)
        {
            richTextBox1.Text += String.Format("[-Message received from {0} in #{1} on {2}: {3}\n", e.author.user.username, e.Channel.name, Program.MainDiscordClient.GetServersList().Find(x=>x.channels.Find(y=>y.id == e.Channel.id) != null).name, e.message);
        }

        private void MainDiscordClient_ChannelCreated(object sender, DiscordSharp.DiscordChannelCreateEventArgs e)
        {
            richTextBox1.Text += String.Format("!! Channel Created !!: {0} on {1}", e.ChannelCreated.name, Program.MainDiscordClient.GetServersList().Find(x=>x.channels.Find(y=>y.id == e.ChannelCreated.id) != null).name);
        }

        private void View_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MainDiscordClient.Dispose();
        }

        private void View_Load(object sender, EventArgs e)
        {
        }
    }
}
