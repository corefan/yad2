using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Yad.Log;
using Yad.Net.Common;
using Yad.Net.Messaging.Common;
using Yad.Log.Common;
using Yad.Utilities.Common;

namespace Yad.Net.Client
{
    public class Connection : IConnection
    {
        private TcpClient tcpClient;
        private MessageReceiver receiver;
        private MessageSender sender;

        private static Connection instance = new Connection();

        private Connection()
        {
            sender = new MessageSender();
            receiver = new MessageReceiver();
            receiver.ConnectionLost += new ConnectionLostEventHandler(receiver_ConnectionLost);
        }

        public static Connection Instance
        {
            get
            { return instance; }
        }

        public void InitConnection(string hostnameWithPort)
        {
            string hostname = hostnameWithPort.Substring(0, hostnameWithPort.Length - 5);
            int port = int.Parse(hostnameWithPort.Substring(hostnameWithPort.Length - 4));

            this.InitConnection(hostname, port);
        }

        public void InitConnection(string hostname, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.NoDelay = true;

                InfoLog.WriteInfo("Connecting to " + hostname + " on port " + port + " ...", EPrefix.ClientInformation);
                tcpClient.Connect(hostname, port);
                NetUtils.SetKeepAlive(tcpClient);
                InfoLog.WriteInfo("Connected succesfully", EPrefix.ClientInformation);

                sender.Stream = tcpClient.GetStream();
                sender.Start();
                InfoLog.WriteInfo("Sender run succesfully", EPrefix.ClientInformation);

                receiver.Stream = tcpClient.GetStream();
                receiver.Start();
            }
            catch (Exception ex)
            {
                InfoLog.WriteException(ex);
                if (ex is SocketException)
                    throw new Exception("Connection exception", ex);
            }
        }
       
        public bool Connected
        {
            get
            {
                if (tcpClient != null)
                    return tcpClient.Connected;
                else
                    return false;
            }
        }

        void receiver_ConnectionLost(object objectSender, EventArgs e)
        {
            receiver.Stop();
            sender.Stop();
        }

        public event ConnectionLostEventHandler ConnectionLost
        {
            add
            { receiver.ConnectionLost += value; }
            remove
            { receiver.ConnectionLost -= value; }
        }

        public event MessageEventHandler MessageReceive
        {
            add
            { receiver.MessageReceive += value; }
            remove
            { receiver.MessageReceive -= value; }
        }

        public event MessageEventHandler MessageSend
        {
            add
            { sender.MessageSend += value; }
            remove
            { sender.MessageSend -= value; }
        }

        public void CloseConnection()
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                sender.Stop();
                receiver.Stop();
                tcpClient.Close();
            }
        }

        public IMessageHandler MessageHandler
        {
            get
            { return receiver.MessageHandler; }
            set
            { receiver.MessageHandler = value; }
        }

        public void SendMessage(Message message)
        {
            if (tcpClient.Connected)
                sender.AddItem(message);
            else
                InfoLog.WriteError("Not connected - cannot send a message", EPrefix.ClientInformation);
        }
	}
}
