using DoorServer.TcpServers;
using Microsoft.Extensions.Logging;

namespace DoorServer.TcpServers.Management
{
    public interface IServerManagementHub
    {
        Task RloginServerStatus(string status);
        
        Task ServerStatusUpdate(string message);

        Task UpdateNodeStatus(int NodeId, ServerType serverType, NodeStatus Status, string? NodeDetail);

        
    }
}
