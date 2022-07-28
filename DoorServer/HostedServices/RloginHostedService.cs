using DoorServer.Common.TcpServers.Rlogin;
using DoorServer.Common.TcpServers.SshTunnel;
using DoorServer.TcpServers.Rlogin.Server;

namespace DoorServer.HostedServices
{
    public class RloginHostedService : BackgroundService, IDisposable
    {
        private readonly RloginServer server;
        public ILogger<RloginHostedService> logger;
        public RloginServerConfiguration serverConfiguration;

        public RloginHostedService(ILogger<RloginHostedService> logger, RloginServerConfiguration serverConfiguration, 
            ISshTunnelConfiguration tunnelConfiguration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serverConfiguration = serverConfiguration;

            if (!this.serverConfiguration.Enabled)
            {
                logger.LogInformation("Rlogin server is disabled.");
                return;
            }

            server = new RloginServer(logger, serverConfiguration, tunnelConfiguration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!serverConfiguration.Enabled)
                return;

            logger.LogDebug("Rlogin server background task is starting.");

            //stoppingToken.Register(() => logger.LogDebug("Rlogin server background task is stopping."));
            server.Start();


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
                catch
                {
                    return;
                }
            }

            logger.LogDebug($"Rlogin server background task is stopping.");

            server.Stop();
            return;
        }

        public void Dispose()
        {
            if (server == null)
                return;

            if (server.IsAccepting)
                server.Stop();

            if (!server.IsDisposed)
                server.Dispose();
        }
    }
}
