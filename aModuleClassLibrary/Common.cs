using System;
using System.Collections.Generic;
using System.Text;

namespace aModuleClassLibrary
{
    public class CommQueueMsg
    {
        public string Command { get; set; }
        public object Data { get; set; }

        public CommQueueMsg(string msg, object data = null)
        {
            Command = msg;
            Data = data;
        }
    }
}
