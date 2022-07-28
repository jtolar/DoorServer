using DoorServer.Common.TcpServers.SshTunnel;
using System.Net;
using System.Text.RegularExpressions;

namespace DoorServer.TcpServers.SshTunnel
{
    public class SshTunnelConfiguration : ISshTunnelConfiguration
    {
        public string? SshHost { get; set; }
        public IPAddress SshHostIpAddress
        {
            get
            {
                if (Regex.Match(SshHost, "^(?:[0-9]{1,3}\\.){3}[0-9]{1,3}$").Success)
                    return IPAddress.Parse(SshHost).MapToIPv4();
                else
                    return Dns.GetHostAddresses(SshHost)[0].MapToIPv4();
            }
        }

        public uint SshPort { get; set; }

        public string? SshUserName { get; set; }
        public string? SshPassword { get; set; }
        public string? ForwardHost { get; set; }
        public uint ForwardPort { get; set; }

        public string SystemTag { get; set; }
    }
}
