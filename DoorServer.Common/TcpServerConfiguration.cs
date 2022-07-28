using DoorServer.Common.TcpServers;
using Newtonsoft.Json;
using System.Net;

namespace DoorServer.Common
{
    public class TcpServerConfiguration : ITcpServerConfiguration
    {
        public TcpServerConfiguration() : this(false, IPAddress.Any, 0)
        {

        }
        protected TcpServerConfiguration(bool enabled, IPAddress ipAddress, int port)
        {
            Enabled = enabled;
            IpAddress = ipAddress.MapToIPv4().ToString();
            Port = port;
        }

        [JsonProperty("Enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("IpAddress")]
        public string IpAddress { get; set; }

        [JsonIgnore]
        public IPAddress ParsedIpAddress
        {
            get => !string.IsNullOrWhiteSpace(IpAddress)
                ? IPAddress.Parse(IpAddress)
                : IPAddress.Any;
        }

        [JsonProperty("Port")]
        public int Port { get; set; }
    }
}
