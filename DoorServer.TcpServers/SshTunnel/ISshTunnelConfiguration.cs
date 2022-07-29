using System.Net;

namespace DoorServer.TcpServers.SshTunnel
{
    public interface ISshTunnelConfiguration
    {
        string? SshHost { get; set; }

        IPAddress SshHostIpAddress { get; }

        string? ForwardHost { get; set; }

        uint ForwardPort { get; set; }

        uint SshPort { get; set; }

        string? SshUserName { get; set; }

        string? SshPassword { get; set; }

        string SystemTag { get; set; }
    }
}