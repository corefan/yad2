using System;
using System.Collections.Generic;
using System.Text;

namespace Yad.Net.Common {
    public class PlayerData : ICloneable
    {
        short _id;
        string _login;
        int _lossNo;
        int _winNo;

        public short Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Login
        {
            get { return _login; }
            set { _login = value; }
        }

        public int WinNo
        {
            get { return _winNo; }
            set { _winNo = value; }
        }

        public int LossNo
        {
            get { return _lossNo; }
            set { _lossNo = value; }
        }

        public override string ToString()
        {
            return "Login: " + _login + Environment.NewLine + "Win: " + _winNo + Environment.NewLine + "Loss: " + _lossNo;
        }

        #region ICloneable Members

        public object Clone() {
            PlayerData pd = new PlayerData();
            pd.Login = this.Login;
            pd.Id = this.Id;
            pd.WinNo = this.WinNo;
            pd.LossNo = this.LossNo;
            return pd;
        }

        #endregion
    }
}
