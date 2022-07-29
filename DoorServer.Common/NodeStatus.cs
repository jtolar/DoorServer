using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorServer.Common
{
    public enum NodeStatus
    {
        Stopped = 0,
        Listening,
        Connecting,
        Connected,
        Disconnecting
    }
}
