
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Yad.Log;
using Yad.Net.Common;
using Yad.Net.Messaging.Common;
using Yad.Log.Common;
using Yad.Net.Utils;
using Yad.Net.Messaging;

namespace Yad.Net.Server {
    
    public delegate void ReceiveMessageDelegate(object sender, ReceiveMessageEventArgs eventArgs);
    public delegate void ConnectionLostDelegate(object sender, ConnectionLostEventArgs eventArgs);

    public class Player : IPlayerID {

        #region Private members

        private BinaryReader _readStream;
        private BinaryWriter _writeStream;
        private Thread _rcvThread;
        private event ReceiveMessageDelegate _onReceiveMessage;
        private event ConnectionLostDelegate _onConnectionLost;
        private short _id;
        private MenuState _state;
        private PlayerData _data;
        private string _gameName;
        private bool _isDisconnected = false;
        private TurnAskProcessCounter _tac = new TurnAskProcessCounter();

        private readonly object _rcvMsgLock = new object();
        private readonly object _clMsgLock = new object();
        private readonly object _syncRoot = new object();

        public object SyncRoot {
            get { return _syncRoot; }
        } 


        #endregion Private members

        #region Properties

        public string GameName {
            get { return _gameName; }
            set { _gameName = value; }
        }

        public short Id {
            get { return _id; }
            set { _id = value; }
        }

        public MenuState State {
            get { return _state; }
            set { _state = value; }
        }

        public string Login {
            get { return _data.Login; }
        }

        public int WinNo {
            get { return _data.WinNo; }
            set { _data.WinNo = value; }
        }

        public int LossNo {
            get { return _data.LossNo; }
            set { _data.LossNo = value; }
        }

        public PlayerData PlayerData {
            get { return _data; }
        }

        public event ConnectionLostDelegate OnConnectionLost {
            add {
                lock (_clMsgLock) {
                    _onConnectionLost += value;
                }
            }
            remove {
                lock (_clMsgLock) {
                    _onConnectionLost -= value;
                }
            }
        }

        public event ReceiveMessageDelegate OnReceiveMessage {
            add {
                lock (_rcvMsgLock) {
                    _onReceiveMessage += value;
                }
            }
            remove 
            {
                lock (_rcvMsgLock) {
                    _onReceiveMessage -= value;
                }
            }
        }

        #endregion Properties

        public Player(short id, TcpClient client) {
            _id = id;
            _readStream = new BinaryReader(client.GetStream());
            _writeStream = new BinaryWriter(client.GetStream());
            _isDisconnected = false;
            _rcvThread = new Thread(new ThreadStart(ReceiveMessages));
        }

        

        public void Start() {
            _rcvThread.Start();
        }

        public void SetData(PlayerData pd) {
            if (pd != null) {
                _data = pd;
                pd.Id = _id;
            }
        }

        public void Stop() {
            _writeStream.Close();
            _rcvThread.Join();
        }

        public void SendMessage(Message msg) {
            try {
                if (!_isDisconnected) {
                    if (msg.Type == MessageType.Build) {
                        BuildMessage bm = (BuildMessage)msg;
                        InfoLog.WriteInfo("Sending build message to player: " + this.Id, EPrefix.Test);
                        InfoLog.WriteInfo("Buildmessage data: " + bm.ToString(), EPrefix.Test);
                    }
                    if (msg.Type == MessageType.BuildUnitMessage) {
                        BuildUnitMessage bum = (BuildUnitMessage)msg;
                        InfoLog.WriteInfo("Sending build unit message to player: " + this.Id, EPrefix.Test);
                        InfoLog.WriteInfo("BuildUnitMessage data: " + bum.ToString(), EPrefix.Test);
                    }
                    msg.Serialize(_writeStream);
                    _writeStream.Flush();
                    if (msg.Type == MessageType.DoTurn)
                        _tac.Unset(this.Login);
                }
            }
            catch (Exception ex) {
                InfoLog.WriteException(ex);
                ExecuteOnConnectionLost();
            }
        }

        private void ExecuteOnConnectionLost() {
            if (!_isDisconnected) {
                _isDisconnected = true;
                lock (_clMsgLock) {
                    if (_onConnectionLost != null)
                        _onConnectionLost(this, new ConnectionLostEventArgs());
                }
            }
        }

        private void ExecuteOnReceiveMessage(Message msg) {
            lock (_rcvMsgLock) {
                if (_onReceiveMessage != null)
                _onReceiveMessage(this, new ReceiveMessageEventArgs(msg));
            }
        }
        public void ReceiveMessages() {
            byte type;
            while (true) {
                try {
                    type = _readStream.ReadByte();
                }
                catch (Exception) {
                    ExecuteOnConnectionLost();
                    return;
                    
                }
                if (type == (byte)MessageType.TurnAsk)
                    _tac.Set();
                if (_data != null)
                    InfoLog.WriteInfo("Player " + this.Login + " sent message", EPrefix.MessageReceivedInfo);
                else
                    InfoLog.WriteInfo("Player unknown sent message", EPrefix.MessageReceivedInfo);
                Message msg = MessageFactory.Create((MessageType)type);
                InfoLog.WriteInfo("Received message with type " + type, EPrefix.MessageReceivedInfo);
                if (null == msg) {
                    InfoLog.WriteInfo("Received unknown message", EPrefix.MessageReceivedInfo);
                    continue;
                }
                try {
                    msg.Deserialize(_readStream);
                }
                catch (Exception) {
                    ExecuteOnConnectionLost();
                    return;
                }
               
                msg.SenderId = Id;
                ExecuteOnReceiveMessage(msg);
                Thread.Sleep(0);
            }
        }



        #region IPlayerID Members

        public short GetID() {
            return this.Id;
        }



        #endregion
    }
}
