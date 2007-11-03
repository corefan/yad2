using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Client.Net {
    public class ConsumerSync {

        private Semaphore _queueSem;
        private WaitHandle[] _eventArray;
        private EventWaitHandle _exitThreadEvent;
        private const int _MaxCount = 99999;
        private const int _WaitIndex = 1;

        public uint WaitIndex {
            get {
                return _WaitIndex;
            }
        }
        public ConsumerSync() {

            _queueSem = new Semaphore(0, _MaxCount);
            _eventArray = new WaitHandle[2];
            _eventArray[0] = _queueSem;
            _eventArray[1] = _exitThreadEvent;
        }

        public void AddObject() {
            _queueSem.Release();
        }

        public EventWaitHandle ExitThreadEvent {
            get { return _exitThreadEvent; }
        }

        public WaitHandle[] EventArray {
            get { return _eventArray; }
        }

        

    }
}