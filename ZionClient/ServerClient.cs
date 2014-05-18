using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.Mentalis.Network.ProxySocket;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace ZionClient
{
    class ServerClient
    {
        private ProxySocket client;
        private Crypto crypto;

        ManualResetEvent resetConnect = new ManualResetEvent(false);
        ManualResetEvent resetDisconnect = new ManualResetEvent(false);
        ManualResetEvent resetRecieve = new ManualResetEvent(false);
        ManualResetEvent resetSend = new ManualResetEvent(false);


        //On Client Connected
        public delegate void onClientConnectHandler(object sender, EventArgs e);
        public event onClientConnectHandler onClientConnect;

        //On Client Disconnected
        public delegate void onClientDisConnectHandler(object sender, EventArgs e);
        public event onClientDisConnectHandler onClientDisConnect;

        //On Data received
        public delegate void onDataReceivedHandler(object sender, DataEventArgs e);
        public event onDataReceivedHandler onDataReceived;

        //On Error received
        public delegate void onErrorHandler(object sender, DataEventArgs e);
        public event onErrorHandler onError;

        public ServerClient()
        {
            try
            {
                client = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                crypto = new Crypto("ssl_server.crt");
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        public void disconnect()
        {
            try
            {
                client.BeginDisconnect(false, new AsyncCallback(onDisconnectCallback), client);
                resetDisconnect.WaitOne();
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        public void connect(bool useTOR = false)
        {
            try
            {
                if (useTOR)
                {
                    client.ProxyEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9150);
                    client.ProxyType = ProxyTypes.Socks5;
                }
                resetConnect.Reset();
                client.BeginConnect("149.210.129.190", 994, new AsyncCallback(onConnectCallback), client);
                resetConnect.WaitOne();
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        private void onConnectCallback(IAsyncResult ar)
        {
            try
            {                
                ProxySocket worker = (ProxySocket)ar.AsyncState;
                if (worker == null)
                {
                    worker = this.client;
                }

                if (worker.Connected == true)
                {                    
                    worker.EndConnect(ar);
                    worker.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                }
                else
                {
                    resetConnect.Reset();
                    //Not connected
                }
                resetConnect.Set();
                onClientConnect(this, new EventArgs());                
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.worker;
                int read = handler.EndReceive(ar);

                if (read > 0)
                {
                    //Store what we have now and receive the rest
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, read));

                    //Event
                    String content = state.sb.ToString();
                                           
                    //There might be more data to receive
                    client.BeginReceive(state.buffer, 0, 256, 0, new AsyncCallback(ReceiveCallback), state);
                    resetRecieve.Set();

                    onDataReceived(this, new DataEventArgs(content));
                }                
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        private void onDisconnectCallback(IAsyncResult ar)
        {
            try
            {
                resetDisconnect.Set();
                ProxySocket worker = (ProxySocket)ar.AsyncState;
                worker.EndDisconnect(ar);
                client.Shutdown(SocketShutdown.Both);
                onClientConnect(this, new EventArgs());
            }
            catch (Exception err)
            {
                client.Shutdown(SocketShutdown.Both);
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        public void Send(string data)
        {
            try
            {

                if (client.Connected == false) { connect(); }
                if (client.Connected == true)
                {

                    //Get reciepients
                    string encodedMessage = data;
                    byte[] buffer = ASCIIEncoding.UTF8.GetBytes(encodedMessage + "\n");

                    StateObject state = new StateObject();
                    state.worker = client;
                    state.buffer = buffer;

                    client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(onSendCallback), state);
                    resetSend.WaitOne();
                }
                else
                {
                    onError(this, new DataEventArgs("Sorry we can't send your message cause you are not connected to the server."));
                }
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }


        private void onSendCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;                
                int bytesSend = state.worker.EndSend(ar);
                resetSend.Set();
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

        public void StartReceive()
        {
            try
            {
                StateObject state = new StateObject();
                state.worker = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
                resetRecieve.Set();
            }
            catch (Exception err)
            {
                onError(this, new DataEventArgs(err.ToString()));
            }
        }

    }

    class StateObject
    {
        public Socket worker { get; set; }
        public StringBuilder sb = new StringBuilder();
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
    }
}
