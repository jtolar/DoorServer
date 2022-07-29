using System.Net;

namespace DoorServer.TcpServers
{
    public interface ITcpServerConfiguration
    {
        bool Enabled { get; set; }
        bool RunOnStartup { get; set; }
        string IpAddress { get; set; }
        int Port { get; set; }
        IPAddress ParsedIpAddress { get; }
    }
}
