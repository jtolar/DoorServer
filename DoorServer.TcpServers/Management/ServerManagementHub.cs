using DoorServer.TcpServers.HostedServices;
using Microsoft.AspNetCore.SignalR;

namespace DoorServer.TcpServers.Management
{
    public class ServerManagementHub : Hub<IServerManagementHub>
    {
        private ILogger logger;
        private RloginHostedService rloginHostedService;

        public ServerManagementHub(ILogger<ServerManagementHub> logger, RloginHostedService rloginHostedService)
        {
            this.logger = logger;
            this.rloginHostedService = rloginHostedService;
        }

        public async Task ToggleServerStartStop()
        {
            if (rloginHostedService != null)
                if (rloginHostedService.IsEnabled && rloginHostedService.ServerIsRunning)
                    //await rloginHostedService.StopServer();
                    await StopRloginServer();
                else
                    //await rloginHostedService.StartServer();
                    await StartRloginServer();

            return;
        }

        public async Task GetRloginStatus()
        {
            await Clients.All.RloginServerStatus(rloginHostedService.ServerIsRunning ? "Started" : "Stopped");
        }

        public async Task ServerStatusUpdate()
        {
            var status = $"Rlogin server is ";

            if (rloginHostedService == null || !rloginHostedService.IsEnabled)
            {
                status += "disabled.";
                await Clients.All.ServerStatusUpdate(status);
                return;
            }

            status += "enabled. ";

            if (rloginHostedService.ServerIsRunning)
                status += $"Server is listening on IP {rloginHostedService.ServerConfiguration.IpAddress} port {rloginHostedService.ServerConfiguration.Port}.";
            else
                status += "Server is currently offline and not accepting connections.";
            await Clients.All.ServerStatusUpdate(status);
            return;
        }

        public async Task<bool> StartRloginServer()
        {
            await rloginHostedService.StartServer();
            await ServerStatusUpdate();
            await GetRloginStatus();
            return rloginHostedService.ServerIsRunning;
        }

        public async Task<bool> StopRloginServer()
        {
            await rloginHostedService.StopServer();
            await ServerStatusUpdate();
            await GetRloginStatus();

            return !rloginHostedService.ServerIsRunning;
        }

        public override async Task OnConnectedAsync()
        {
            await ServerStatusUpdate();
            await GetRloginStatus();
            await base.OnConnectedAsync();
        }
    }
}
