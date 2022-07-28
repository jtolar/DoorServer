using System.Net;

namespace DoorServer.Common.TcpServers
{
    public interface ITcpServerConfiguration
    {
        bool Enabled { get; set; }
        string IpAddress { get; set; }
        int Port { get; set; }
        IPAddress ParsedIpAddress { get; }
    }
}
