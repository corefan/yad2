using System;
using System.Collections.Generic;
using System.Text;
using Yad.Net.Common;
using System.Collections;
using Yad.Net.Messaging.Common;
using Yad.Log.Common;
using Yad.Properties;
using Serv = Yad.Net.GameServer.Server;
using Yad.Net.GameServer.Server;

namespace Yad.Net.Server {
     
    class GameManager {

        #region Private Members

        private const short MaxGameNumber = 20;
        private Dictionary<short, Player> _players = null;
        private Dictionary<string, ServerGameInfo> _games = null;
        private IMessageSender _sender = null;
        private List<Serv.GameServer> _gameServerList = null;
        private Server _server = null;

        #endregion

        #region Constructors 

        public GameManager(Server server, IMessageSender sender) {

            _players = new Dictionary<short, Player>();
            _games = new Dictionary<string, ServerGameInfo>();
            _gameServerList = new List<Yad.Net.GameServer.Server.GameServer>();

            _sender = sender;
            _server = server;

        }

        #endregion

        #region Public Methods

        public void AddPlayer(Player player) {
            lock (((ICollection)_players).SyncRoot) {
                _players.Add(player.Id, player);
            }
            SendGameListMessage(player.Id);
        }

        public void RemovePlayer(short id) {
            lock (((ICollection)_players).SyncRoot) {
                _players.Remove(id);
            }
        }

        public void ModifyPlayer(short id, PlayerInfo pi) {
            lock (((ICollection)_players).SyncRoot) {
                Player p = _players[id];
                if (p == null || p.GameName == null)
                    return;
                lock (((ICollection)_games).SyncRoot) {
                    _games[p.GameName].ModifyPlayer(id, pi);
                }
            }
        }

        public void RemoveFromGameJoin(Player player) {
            lock (((ICollection)_games).SyncRoot) {
                foreach (ServerGameInfo sgi in _games.Values)
                    if (sgi.IsInside(player.GetID())) {
                        sgi.RemovePlayer(player);
                        player.GameName = null;
                        lock (sgi) {
                            if (sgi.PlayerNo == 0) {
                                RemoveGame(sgi);
                                SendRemoveGameMessage(sgi);
                            }
                        }
                        break;
                    }
            }
        }

        public void StartGame(short playerid) {
            lock (((ICollection)_players).SyncRoot) {
                if (_players.ContainsKey(playerid)) {
                    string name = _players[playerid].GameName;
                    if (name == null)
                        return;
                    lock (((ICollection)_games).SyncRoot) {
                        if (_games.ContainsKey(name)) {
                            if (_games[name].StartClick(playerid)) {
                                PreparePlayersForServerGame(_games[name]);
                                CreateServerGame(_games[name]);
                                _games.Remove(name);
                            }
                        }
                    }
                }
            }
        }
        
       
        public void StopGames() {
            lock (((ICollection)_gameServerList).SyncRoot) {
                foreach (Serv.GameServer gs in _gameServerList)
                    gs.StopGameServer();
            }
        }

        #endregion

        private void PreparePlayersForServerGame(ServerGameInfo sgi) {
            lock (((ICollection)_players).SyncRoot) {
                foreach (IPlayerID pid in sgi.Players.Values) {
                    Player p = _players[pid.GetID()];
                    p.State = PlayerStateMachine.Transform(p.State, MenuAction.GameStart);
                    _players.Remove(p.Id);
                }
            }
        }
        private void CreateServerGame(ServerGameInfo sgi)
        {
           
            Serv.GameServer gs = new Yad.Net.GameServer.Server.GameServer(sgi);
            gs.EnterMsgHandlerChange(this._server);
            lock (((ICollection)_gameServerList).SyncRoot)
            {
                _gameServerList.Add(gs);
            }
            gs.OnGameEnd += new GameEndDelegate(OnGameServerEnd);
            gs.OnPlayerLeave +=new PlayerGameLeaveDelegate(OnPlayerGameLeave);
            gs.StartGameServer();
        }

        private void OnPlayerGameLeave(object sender, EventArgs ea)
        {
            Player p = sender as Player;
            p.State = PlayerStateMachine.Transform(p.State, MenuAction.GameEnd);
            _server.Chat.AddPlayer(p);
            p.OnReceiveMessage += new ReceiveMessageDelegate(_server.MessageHandler.OnReceivePlayerMessage);
        }
        private void OnGameServerEnd(object sender, GameEndEventArgs args)
        {
            Serv.GameServer gs = sender as Serv.GameServer;
            Player[] players = gs.GetPlayersArray();
            gs.LeaveMsgHandlerChange(this._server);
            for (int i = 0; i < players.Length; ++i)
            {
                players[i].State = PlayerStateMachine.Transform(players[i].State,
                    MenuAction.GameEnd);
                _server.Chat.AddPlayer(players[i]);
            }
            _gameServerList.Remove(gs);
        }

        private void RemoveGame(ServerGameInfo sgi) {
            lock (((ICollection)_games).SyncRoot) {
                _games.Remove(sgi.Name);
            }
            InfoLog.WriteInfo(string.Format(Resources.ActionStringFormat, "game removal"), EPrefix.ServerAction);
            InfoLog.WriteInfo("Removed game: " + sgi.Name, EPrefix.ServerAction);
        }

        private void SendGameListMessage(short recipient) {
            List<GameInfo> list = new List<GameInfo>();
            lock (((ICollection)_games).SyncRoot) {
                foreach (ServerGameInfo sgi in _games.Values)
                    if (!sgi.IsPrivate)
                        list.Add(sgi.GetGameInfo());
            }
           
            GamesMessage msg = MessageFactory.Create(MessageType.Games) as GamesMessage;
            msg.ListGameInfo = list;
            msg.Operation = (byte)MessageOperation.List;
            _sender.MessagePost(msg, recipient);
        }

        private void SendRemoveGameMessage(ServerGameInfo sgi) {

            GamesMessage msg = CreateBasicGamesMsg(sgi);
            msg.Operation = (byte)MessageOperation.Remove;
            BroadcastExcl(msg, -1);
            
        }

        private static GamesMessage CreateBasicGamesMsg(ServerGameInfo sgi) {
            List<GameInfo> list = new List<GameInfo>();
            list.Add(sgi.GetGameInfo());
            GamesMessage msg = MessageFactory.Create(MessageType.Games) as GamesMessage;
            msg.ListGameInfo = list;
            return msg;
        }

        private void SendJoinGameMessage(ServerGameInfo sgi, short id) {
            GameInfoMessage msg = MessageFactory.Create(MessageType.GameParams) as GameInfoMessage;
            msg.GameInfo = sgi.GetGameInfo();
            SendMessage(msg, id);
        }
        private void SendCreateGameMessage(ServerGameInfo sgi) {
            GamesMessage msg = CreateBasicGamesMsg(sgi);
            msg.Operation = (byte)MessageOperation.Add;
            BroadcastExcl(msg, -1);
        }

        public ResultType CreateGame(GameInfo gi) {
            if (GameNumber == MaxGameNumber)
                return ResultType.MaxServerGameError;
            ResultType result;
            lock (((ICollection)_games).SyncRoot) {
                result = IsValid(gi);
                if (result != ResultType.Successful)
                    return result;
                ServerGameInfo sgi = new ServerGameInfo(gi, _sender);
                _games.Add(gi.Name, sgi);
                SendCreateGameMessage(sgi);
                InfoLog.WriteInfo(string.Format(Resources.ActionStringFormat, "create game"), EPrefix.ServerAction);
                InfoLog.WriteInfo("Created game: " + gi.Name, EPrefix.ServerAction);
            }
            return result;
        }

        public void CreateGameNoError(GameInfo gi) {
            lock (((ICollection)_games).SyncRoot) {
                ServerGameInfo sgi = new ServerGameInfo(gi, _sender);
                _games.Add(gi.Name, sgi);
                SendCreateGameMessage(sgi);
                InfoLog.WriteInfo(string.Format(Resources.ActionStringFormat, "create game"), EPrefix.ServerAction);
                InfoLog.WriteInfo("Created game: " + gi.Name, EPrefix.ServerAction);
            }
        }

       

        public ResultType IsCreateGamePossible(GameInfo gi){
            if (GameNumber == MaxGameNumber)
                return ResultType.MaxServerGameError;
            lock (((ICollection)_games).SyncRoot) {
                return IsValid(gi);
            }
        }

        protected void BroadcastExcl(Message msg, short id) {
            lock (((ICollection)_players).SyncRoot)
                foreach (IPlayerID pid in _players.Values)
                    if (pid.GetID() != id)
                        _sender.MessagePost(msg, pid.GetID());
        }

        public ResultType IsJoinGamePossible(string name, Player player) {
            lock (((ICollection)_games).SyncRoot) {
                return IsJoinPossible(name);
            }
        }
        public void JoinGame(string name, Player player) {
            SendJoinGameMessage(_games[name], player.Id);
            _games[name].AddPlayer(player);
            player.GameName = name;
            InfoLog.WriteInfo("Player " + player.Login + "has joined game: " + name, EPrefix.ServerAction);
        }

        private void SendMessage(Message msg, short id) {
            _sender.MessagePost(msg, id);
        }
        private short GameNumber {
            get {
                return (short)_games.Count;
            }
        }
        private ResultType IsJoinPossible(string name) {
            //czy nazwa istnieje
            if (!_games.ContainsKey(name))
                return ResultType.GameNotExists;
           //czy gra jest pelna
            if (!_games[name].IsAddPosible())
                return ResultType.GameFull;
            //czy gra sie zaczela - jak bedzie wiadomo jak rozpoczac gre :)
            return ResultType.Successful;
           
        }

        public ResultType IsValid(GameInfo gi) {
            if (!IsNameValid(gi.Name))
                return ResultType.NameExistsError;
            if (!IsPlayerNoValid(gi.MaxPlayerNumber))
                return ResultType.InvalidPlayerNoError;
            if (!IsMapValid(gi.MapName))
                return ResultType.InvalidMapIdNoError;
            return ResultType.Successful;
            
        }

        public bool IsNameValid(string gameName) {
            if (_games.ContainsKey(gameName))
                return false;
            return true;
        }

        public bool IsPlayerNoValid(int playerNo) {
            return true;
        }

        public bool IsMapValid(string mapName) {
            //TODO (PW) check if file with specified name excists on server
            return true;
        }
    }
}
