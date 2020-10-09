﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChattingClient
{
    public class ChattingThreadData
    {
        public static int chattingRoomCnt = 0;
        public Thread chattingThread;
        public ChattingWindow chattingWindow;
        public int chattingRoomNum;
        private static object obj = new object();

        public ChattingThreadData(Thread chattingThread, ChattingWindow chattingWindow)
        {
            lock (obj)
            {
                this.chattingThread = chattingThread;
                this.chattingWindow = chattingWindow;
                this.chattingRoomNum = ++chattingRoomCnt;
            }
        }
    }
}
