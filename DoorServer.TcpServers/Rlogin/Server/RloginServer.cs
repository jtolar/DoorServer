using DoorServer.Common.TcpServers.Rlogin;
using DoorServer.Common.TcpServers.SshTunnel;

namespace DoorServer.TcpServers.Rlogin.Server
{
    public class RloginServer : TcpServer
    {
        public ILogger logger;
        public ISshTunnelConfiguration tunnelConfiguration;
        public RloginServerConfiguration serverConfiguration;

        public RloginServer(ILogger logger, RloginServerConfiguration serverConfiguration, 
            ISshTunnelConfiguration tunnelConfiguration) :
            base(serverConfiguration.ParsedIpAddress, serverConfiguration.Port)
        {
            this.logger = logger ?? throw new ArgumentException(nameof(logger));
            this.tunnelConfiguration = tunnelConfiguration;
            this.serverConfiguration = serverConfiguration;
            SessionDictionary = new Dictionary<Guid, TcpSession>();
        }

        public Dictionary<Guid, TcpSession> SessionDictionary { get; set; }

        protected override void OnDisconnected(TcpSession session)
        {
            base.OnDisconnected(session);
        }

        protected override void OnStarting()
        {
            logger.LogInformation($"Rlogin server is starting. Maximum connections allowed is {serverConfiguration.MaxConnections}");
            base.OnStarting();
        }

        protected override void OnStarted()
        {
            var logMsg = "Rlogin server has started on ";

            if (serverConfiguration.ParsedIpAddress == IPAddress.Any)
                logMsg += $"all IP Addresses";
            else if (serverConfiguration.ParsedIpAddress == IPAddress.Loopback)
                logMsg += "localhost";
            else
                logMsg += $"IP Address {serverConfiguration.ParsedIpAddress.ToString()}";

            logMsg += $" using port {serverConfiguration.Port}.";

            logger.LogInformation(logMsg);
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            logger.LogInformation("Rlogin server has been stopped.");
            base.OnStopped();
        }

        protected override void OnStopping()
        {
            logger.LogInformation("Rlogin server is stopping.");
            base.OnStopping();
        }

        protected override TcpSession CreateSession()
        {
            return new RLoginSession(logger, this, tunnelConfiguration);
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            logger.LogError($"Rlogin Server caught an error code {error}.\n{Enum.GetName(typeof(System.Net.Sockets.SocketError), error)}");
        }
    }
}
