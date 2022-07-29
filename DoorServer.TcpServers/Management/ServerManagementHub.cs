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

        public Task<bool> DisconnectNode(int nodeId)
        {
            return Task.FromResult(true);
        }

        public Task LogMessage(LogLevel level, string category, string message)
        {

            return Task.CompletedTask;
        }

        public async Task GetRloginStatus(ServerType serverType, TcpServerStatus status, TcpServerConfiguration serverConfiguration)
        {
            //await Clients.All.UpdateServerStatus(serverType, status);
            return;
        }

        public async Task<bool> StartRloginServer()
        {
            await rloginHostedService.StartAsync(CancellationToken.None);
            return rloginHostedService.ServerIsRunning;
        }

        public async Task<bool> StopRloginServer()
        {
            await rloginHostedService.StopAsync(CancellationToken.None);
            return !rloginHostedService.ServerIsRunning;
        }

        public Task UpdateNodeStatus(int NodeId, ServerType serverType, NodeStatus Status, string? NodeDetail)
        {
            return Task.CompletedTask;
        }

        public Task OnConnected()
        {
            throw new NotImplementedException();
        }

        public Task OnReconnected()
        {
            throw new NotImplementedException();
        }

        public Task OnDisconnected(bool stopCalled)
        {
            throw new NotImplementedException();
        }
    }
}
