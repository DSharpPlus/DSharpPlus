using DiscordSharp;
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
    public partial class LoginForm : Form
    {
        Properties.Settings SettingsInstance = new Properties.Settings();


        public LoginForm()
        {
            InitializeComponent();
            if(SettingsInstance.email != "" && SettingsInstance.password != "")
            {
                textBox1.Text = SettingsInstance.email;
                textBox2.Text = SettingsInstance.password;
                Connect(textBox1.Text, textBox2.Text);
            }
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Connect(string username, string password)
        {
            Program.MainDiscordClient.LoginInformation.email[0] = username;
            Program.MainDiscordClient.LoginInformation.password[0] = password;

            Program.MainDiscordClient.SendLoginRequest();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != null && textBox2.Text != null)
            {
                SettingsInstance.email = textBox1.Text;
                SettingsInstance.password = textBox2.Text;
                SettingsInstance.Save();
                
                Connect(textBox1.Text, textBox2.Text);
            }
        }
    }
}
