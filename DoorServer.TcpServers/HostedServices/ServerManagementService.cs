using DoorServer.TcpServers.Management;
using Microsoft.AspNetCore.SignalR;

namespace DoorServer.TcpServers.HostedServices
{
    public class ServerManagementService 
    {
        private readonly IHubContext<ServerManagementHub, IServerManagementHub> hub;

        public ServerManagementService(IHubContext<ServerManagementHub, IServerManagementHub> hub)
        {
            this.hub = hub;
        }

    }
}
