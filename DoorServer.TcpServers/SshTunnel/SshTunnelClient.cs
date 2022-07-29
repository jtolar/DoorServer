using DoorServer.TcpServers.Rlogin.Server;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace DoorServer.TcpServers.SshTunnel
{

    public class SshTunnelClient
    {
        private SshClient client;
        private RloginConnectionInfo rloginConnection;
        private ConnectionInfo sshConnectionInfo;
        private ForwardedPortLocal? forwardedPort;
        private ISshTunnelConfiguration tunnelConfiguration;

        public SshTunnelClient(RloginConnectionInfo rloginConnection, ISshTunnelConfiguration tunnelConfiguration)
        {
            this.tunnelConfiguration = tunnelConfiguration;

            sshConnectionInfo = new ConnectionInfo(tunnelConfiguration.SshHost, (int)tunnelConfiguration.SshPort,
                tunnelConfiguration.SshUserName, new PasswordAuthenticationMethod(
                    tunnelConfiguration.SshUserName, tunnelConfiguration.SshPassword));

            this.rloginConnection = rloginConnection;

            client = new SshClient(sshConnectionInfo);
            client.ErrorOccurred += Client_ErrorOccurred;
            client.HostKeyReceived += Client_HostKeyReceived;
        }

        public delegate void SshProxyEventHandler(SshClient sshClient);
        public delegate void SshProxyErrorHandler(Exception ex);
        public delegate void SshForwardRequestHandler(PortForwardEventArgs e);
        public delegate void EmptyEventHandler();

        public event EmptyEventHandler? OnClientConnecting;
        public event SshProxyEventHandler? OnClientConnected;
        public event SshProxyEventHandler? OnClientDisconnecting;
        public event EmptyEventHandler? OnClientDisconnected;
        public event SshProxyEventHandler? OnTunnelConnected;
        public event SshProxyEventHandler? OnTunnelConnecting;
        public event EmptyEventHandler? OnTunnelDisconnected;
        public event EmptyEventHandler? OnTunnelDisconnecting;
        public event EmptyEventHandler? OnTunnelCreation;
        public event EmptyEventHandler? OnTunnelCreated;
        public event SshProxyErrorHandler? OnTunnelError;
        public event SshProxyErrorHandler? OnClientConnectionError;
        public event SshForwardRequestHandler? OnForwardRequestReceived;

        public bool IsTunnelConnected { get; set; }
        public bool IsSshServerConnected { get => client.IsConnected; }

        private void CreateSshConnection()
        {
            OnClientConnecting?.Invoke();
            try
            {
                client.Connect();

                if (client.IsConnected)
                {
                    OnClientConnected?.Invoke(client);
                }
            }
            catch (Exception ex)
            {
                OnClientConnectionError?.Invoke(ex);
            }
        }

        private void CloseSshConnection()
        {
            try
            {
                OnClientDisconnecting?.Invoke(client);

                if (client.IsConnected)
                    client.Disconnect();

                OnClientDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                OnClientConnectionError?.Invoke(ex);
            }
        }

        public void CreateTunnel()
        {
            try
            {
                OnTunnelCreation?.Invoke();

                if (!client.IsConnected)
                    CreateSshConnection();

                forwardedPort = new ForwardedPortLocal("127.0.0.1", rloginConnection.RemotePort,
                    tunnelConfiguration.ForwardHost, tunnelConfiguration.ForwardPort);

                forwardedPort.Exception += Forward_Exception;
                forwardedPort.RequestReceived += Forward_RequestReceived;

                OnTunnelCreated?.Invoke();
            }
            catch (Exception ex)
            {
                OnTunnelError?.Invoke(ex);
            }
        }

        public void StartTunnel()
        {
            try
            {
                OnTunnelConnecting?.Invoke(client);

                client.AddForwardedPort(forwardedPort);
                //ConsoleWrite.SshTunnelClientMessage($"Tunnel Forward to {forwardedPort.Host}:{forwardedPort.Port}");
                forwardedPort.Start();

                OnTunnelConnected?.Invoke(client);
            }
            catch (Exception ex)
            {
                OnTunnelError?.Invoke(ex);
            }
        }

        public void StopTunnel()
        {
            try
            {
                OnTunnelDisconnecting?.Invoke();
                if (forwardedPort != null)
                    forwardedPort.Stop();

                if (client.ForwardedPorts.Contains(forwardedPort))
                    client.RemoveForwardedPort(forwardedPort);

                OnTunnelDisconnected?.Invoke();

                CloseSshConnection();
            }
            catch (Exception ex)
            {
                OnTunnelError?.Invoke(ex);
            }
        }

        void Client_HostKeyReceived(object? sender, HostKeyEventArgs e)
        {
            //For now we trust everyone. This may change later.
            e.CanTrust = true;
        }

        void Client_ErrorOccurred(object? sender, ExceptionEventArgs e)
        {
            OnClientConnectionError?.Invoke(e.Exception);
        }

        void Forward_Exception(object? sender, ExceptionEventArgs e)
        {
            OnTunnelError?.Invoke(e.Exception);
        }

        void Forward_RequestReceived(object? sender, PortForwardEventArgs e)
        {
            OnForwardRequestReceived?.Invoke(e);
        }
    }
}
