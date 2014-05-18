using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Runtime.Serialization;

namespace ZionClient
{
    public partial class Form1 : Form
    {
        private ServerClient client;

        public Form1()
        {
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();            
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.disconnect();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Check is process is already running
            var me = Process.GetCurrentProcess();
            var arrProcesses = Process.GetProcessesByName(me.ProcessName);
            if (arrProcesses != null && arrProcesses.Length > 1) { MessageBox.Show("Application is already running"); Application.Exit(); }

            client = new ServerClient();            
            client.onClientConnect += new ServerClient.onClientConnectHandler(client_onClientConnect);
            client.onDataReceived += new ServerClient.onDataReceivedHandler(client_onDataReceived);
            client.onClientDisConnect += new ServerClient.onClientDisConnectHandler(client_onClientDisConnect);
            client.onError += new ServerClient.onErrorHandler(client_onError);
            client.connect();
        }

        void client_onError(object sender, DataEventArgs e)
        {
            MessageBox.Show(e.Data);
        }

        void client_onClientDisConnect(object sender, EventArgs e)
        {
            updateChatLog("Client disconnected\r\n");
            client.disconnect();
        }

        void client_onDataReceived(object sender, DataEventArgs e)
        {
            updateChatLog(e.Data + "\r\n");
        }

        void client_onClientConnect(object sender, EventArgs e)
        {            
            client.StartReceive();
        }

        private void cmdSend_Click(object sender, EventArgs e)
        {
            client.Send("{\"command\":\"/msg\",\"channel\":\"private\",\"message\":\"RxMup750Rz0pyv77YsJ7qyyJgpR9O/Rgwfrl1a86ZH277w6fK9pdCUCFqUvg+CfED2NOIpUsX/rteWW2oCmL95zJiTAR6Qzse9PtPsM6RU6aeUmrC8M0NhdHthUA+AT1lcs5+LlV9IWgKqEpeNzeWfRSHfICpOM+QNM6tTSU2cHdJzHKTpphSXjfCISEQj0sOic9ehjjjf6Zh455v49clNMoaCE3RJMRbd+w486QBKY233vgvuwV8X6G38kULoipEd8MDqOLewM1vqXXbCVvDkQoVRuoPNatL4tyL80Fh4RbT2fpnjwUq4oyall73FhIMtWToz3Fs9A1SHzo3HAFNw==\",\"message_type\":\"C\"}");
        }

        private delegate void updateChat(string data);
        private void updateChatLog(string data)
        {
            if (txtData.InvokeRequired)
            {
                updateChat del = new updateChat(updateChatLog);
                txtData.Invoke(del, new object[] { data });
            }
            else
            {
                txtData.Text += data;
            }
        }

    }
}
