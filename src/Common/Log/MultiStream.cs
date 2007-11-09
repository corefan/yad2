using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Yad.Log.Common
{
    public delegate void OnWriteLineDelegate(string s);
    public class MultiStream : IDisposable
    {
        StreamWriter _writer = null;
        event OnWriteLineDelegate _onWriteLine = null;

        public OnWriteLineDelegate OnWriteLine
        {
            get { return _onWriteLine; }
            set { _onWriteLine = value; }
        }
        
        public MultiStream(string filepath)
        {
            try
            {
                _writer = new StreamWriter(filepath, true);
            }
            catch (IOException)
            {
                MessageBox.Show("Cannot open ErrorLog.txt file");
            }
        }

        public MultiStream()
        {
        }

        public void WriteLine(string s)
        {
            if (_onWriteLine != null)
                _onWriteLine(s);
            if (_writer != null) {
                _writer.WriteLine(s);
                _writer.Flush();
            }

			//K�:
			Console.Out.WriteLine(s);
        }

        public void Close()
        {
            _writer.Close();
        }

        #region IDisposable Members

        public void Dispose()
        {
            _writer.Close();
            _writer.Dispose();
        }

        #endregion
    }
}
