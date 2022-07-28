using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoorServer.Common.TcpServers
{
    public class LoginServerConfiguration : TcpServerConfiguration, ILoginServerConfiguration
    {
        public int MaxConnections { get ; set; }
    }
}
