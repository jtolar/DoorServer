using DoorServer.TcpServers;
using Microsoft.Extensions.Logging;

namespace DoorServer.TcpServers.Management
{
    public interface IServerManagementHub
    {
        Task LogMessage(LogLevel level, string category, string message);

        Task<bool> StartRloginServer();

        Task<bool> StopRloginServer();

        Task<bool> DisconnectNode(int nodeId);

        Task UpdateNodeStatus(int NodeId, ServerType serverType, NodeStatus Status, string? NodeDetail);

        Task UpdateServerStatus(string serverType, string status);
    }
}
