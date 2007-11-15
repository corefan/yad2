using System;
using System.Collections.Generic;
using System.Text;
using Yad.Net.Common;

namespace Yad.Net.Client
{
    static class ClientPlayerInfo
    {
        private static short playerId = -1;
        private static string login = string.Empty;
        private static GameInfo gameInfo = null;

        public static short PlayerId
        {
            get
            { return playerId; }
            set
            { playerId = value; }
        }

        public static string Login
        {
            get
            { return login; }
            set
            { login = value; }
        }

        public static string ChatPrefix
        {
            get
            { return "[" + Login + "] : ";  }
        }

        public static GameInfo GameInfo
        {
            get
            { return gameInfo; }
            set
            { gameInfo = value; }
        }
    }
}
