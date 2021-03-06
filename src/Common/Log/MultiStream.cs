using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Yad.UI.Common;

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
                //MessageBoxEx.Show("Cannot open ErrorLog.txt file");
                Console.Out.WriteLine("ERROR: Cannot open ErrorLog.txt file, output only on screen");
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
                try {
                    _writer.WriteLine(s);
                    _writer.Flush();
                } catch (ObjectDisposedException ) {
                    System.Console.Out.WriteLine("stream disposed");
                }
            }

			Console.Out.WriteLine(s);
        }

        public void Close()
        {
            if (_writer != null)
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
