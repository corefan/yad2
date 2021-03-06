using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Client.Log;
using System.Collections;
using System.Windows.Forms;
using Server.Net.General.Server;

namespace Yad.Net.Server {
    /*
     *
    class Server {

        private int _portNumber;
        private TcpListener _listener;

        private Dictionary<int, Player> _playersUnlogged;
        private Dictionary<int, Player> _playersLogged;
        private Chat _chat;

        internal Chat Chat {
            get { return _chat; }
            set { _chat = value; }
        }

        internal Dictionary<int, Player> PlayersUnlogged {
            get { return _playersUnlogged; }
            set { _playersUnlogged = value; }
        }

        private MenuMessageHandler _menuMsgHandler;
        private bool _serverEnd = false;
        private int playerID = 1;

        public Player GetPlayerFromUnlogged(int key) {
            lock (_playersUnlogged) {
                if (_playersUnlogged.ContainsKey(key))
                    return _playersUnlogged[key];
            }
            return null;
        }

        public Server(int PortNumber) {

            _portNumber = PortNumber;
            _playersUnlogged = new Dictionary<int, Player>();
            _playersLogged = new Dictionary<int, Player>();
            _listener = new TcpListener(_portNumber);
            _listener.Start();
            InfoLog.WriteInfo("Server listnening started successfully", EPrefix.ServerInformation);
            _menuMsgHandler = new MenuMessageHandler(this);
            _menuMsgHandler.Start();
            _chat = new Chat();
            InfoLog.WriteInfo("Server menu message handling started successfully", EPrefix.ServerInformation);

        }

        public void Start() {
            while (!_serverEnd) {
                    AcceptConnections();
            }
            _menuMsgHandler.Stop();
            
        }

        public void Stop() {
            _listener.Stop();
        }


        public void OnConnectionLost(object sender, ConnectionLostEventArgs args) {
            Player player = sender as Player;
            InfoLog.WriteInfo("Player " + player.Id + " has disconnected", EPrefix.ServerInformation);
            if (player.State == Yad.Net.General.MenuState.Unlogged)
                _playersUnlogged.Remove(player.Id);
        }
        public void AcceptConnections() {
            TcpClient client = null;
            try {
                client = _listener.AcceptTcpClient();
            }
            catch (Exception) {
                _serverEnd = true;
                return;
            }
            InfoLog.WriteInfo("Server accepted new client");
            Player player = new Player(playerID + 1, client);
            player.OnReceiveMessage += new ReceiveMessageDelegate(_menuMsgHandler.OnReceivePlayerMessage);
            player.OnConnectionLost += new ConnectionLostDelegate(OnConnectionLost);
            player.Start();
            lock(((ICollection)_playersUnlogged).SyncRoot)
                _playersUnlogged.Add(++playerID, player);

        }*/

        class Server : BaseServer {

            private int _portNumber;
            private TcpListener _listener;

            private Dictionary<int, Player> _playersUnlogged;

            private Chat _chat;

            internal Chat Chat {
                get { return _chat; }
                set { _chat = value; }
            }

            internal Dictionary<int, Player> PlayersUnlogged {
                get { return _playersUnlogged; }
                set { _playersUnlogged = value; }
            }

            private bool _serverEnd = false;
            private int playerID = 0;

            public Player GetPlayerUnlogged(int key) {
                lock (_playersUnlogged) {
                    if (_playersUnlogged.ContainsKey(key))
                        return _playersUnlogged[key];
                }
                return null;
            }

            public Server(int PortNumber) : base() {

                _portNumber = PortNumber;
                _listener = new TcpListener(_portNumber);
                _listener.Start();
                InfoLog.WriteInfo("Server listnening started successfully", EPrefix.ServerInformation);

                _playersUnlogged = new Dictionary<int, Player>();
                _playerCollection = new Dictionary<int, Player>();

                _msgHandler = new MenuMessageHandler(this);
                _msgHandler.SetSender(_msgSender);
                StartMessageProcessing();

                _chat = new Chat(_msgSender);
                InfoLog.WriteInfo("Server menu message handling started successfully", EPrefix.ServerInformation);
                
            }

            public void Start() {
                while (!_serverEnd) {
                    AcceptConnections();
                }
                StopMessageProcessing();

            }

            public void Stop() {
                _listener.Stop();
            }

            public override Player GetPlayer(int id)
            {
                Player p = base.GetPlayer(id);
                if (null == p)
                    if (_playersUnlogged != null)
                        return this.GetPlayerUnlogged(id);
                return p;
            }

            


            public void OnConnectionLost(object sender, ConnectionLostEventArgs args) {
                Player player = sender as Player;
                InfoLog.WriteInfo("Player " + player.Id + " has disconnected", EPrefix.ServerInformation);
                if (player.State == Yad.Net.General.MenuState.Unlogged)
                    _playersUnlogged.Remove(player.Id);
                else
                    _playerCollection.Remove(player.Id);
            }

            public void AcceptConnections() {
                TcpClient client = null;
                try {
                    client = _listener.AcceptTcpClient();
                }
                catch (Exception) {
                    _serverEnd = true;
                    return;
                }
                InfoLog.WriteInfo("Server accepted new client");
                Player player = new Player(playerID + 1, client);
                player.OnReceiveMessage += new ReceiveMessageDelegate(_msgHandler.OnReceivePlayerMessage);
                player.OnConnectionLost += new ConnectionLostDelegate(OnConnectionLost);
                player.Start();
                lock (((ICollection)_playersUnlogged).SyncRoot)
                    _playersUnlogged.Add(++playerID, player);

        }

        
        


    }
}
