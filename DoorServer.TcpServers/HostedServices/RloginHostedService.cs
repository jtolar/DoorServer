using DoorServer.TcpServers.Rlogin.Server;
using DoorServer.TcpServers.SshTunnel;
using Microsoft.Extensions.Hosting;

namespace DoorServer.TcpServers.HostedServices
{
    public class RloginHostedService : IHostedService, IDisposable
    {
        private RloginServer? server;
        private ISshTunnelConfiguration tunnelConfiguration;
        private readonly IServiceProvider services;
        private volatile bool isReady = false;
        private volatile bool isRunning = false;
        private volatile bool isStarting = true;

        public ILogger<RloginHostedService> logger;

        public RloginHostedService(ILogger<RloginHostedService> logger, RloginServerConfiguration serverConfiguration,
            IServiceProvider services, IHostApplicationLifetime lifetime, ISshTunnelConfiguration tunnelConfiguration)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ServerConfiguration = serverConfiguration;
            this.tunnelConfiguration = tunnelConfiguration;
            this.services = services;

            if (!this.ServerConfiguration.Enabled)
            {
                logger.LogInformation("Rlogin server is disabled.");
                return;
            }

            server = new RloginServer(logger, serverConfiguration, tunnelConfiguration);

            lifetime.ApplicationStarted.Register(() =>
            {
                isReady = true;
                isStarting = false;
                if (serverConfiguration.Enabled && serverConfiguration.RunOnStartup)
                {
                    StartServer();
                }
                else
                {
                    logger.LogInformation("Rlogin Server is not set to run on startup. You must manually start the rlogin service.");
                }
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                isReady = false;
                StopServer();
            });
        }

        public RloginServerConfiguration ServerConfiguration { get; set; }

        public bool ServerIsRunning => isRunning;

        public bool IsEnabled => ServerConfiguration.Enabled;

        public Task StartServer()
        {
            if (!ServerConfiguration.Enabled)
                return Task.CompletedTask;

            if (isReady && !isRunning)
            {
                if (server == null)
                    server = new RloginServer(logger, ServerConfiguration, tunnelConfiguration);

                server.Start();
                isRunning = server.IsStarted;
            }

            return Task.CompletedTask;
        }

        public Task StopServer()
        {
            if (!ServerConfiguration.Enabled)
                return Task.CompletedTask;

            if (isReady && isRunning && server != null)
            {
                server.Stop();
                isRunning = server.IsStarted;
            }

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopServer();
            return Task.CompletedTask;
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
