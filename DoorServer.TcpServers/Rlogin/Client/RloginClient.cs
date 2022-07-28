using DoorServer.TcpServers.Rlogin.Server;
using System.Text;

namespace DoorServer.TcpServers.Rlogin.Client
{
    public class RloginClient : TcpClient
    {
        private bool rloginSessionEstablished;
        private ILogger logger;

        public RloginClient(ILogger logger, IPAddress address, int port) : base(address, port)
        {
            this.logger = logger;
        }

        public delegate void RloginClientReceivedHandler(byte[] buffer, long offset, long size);
        public delegate void RloginClientEmptyEventHandler();
        public delegate void RloginClientErrorHandler(Exception ex);

        public event RloginClientReceivedHandler? OnDataReceived;
        public event RloginClientEmptyEventHandler? OnClientConnected;
        public event RloginClientEmptyEventHandler? OnClientDisconnected;
        public event RloginClientErrorHandler? OnClientError;

        protected override void OnConnected()
        {
            logger.LogInformation($"Rlogin Client is connected to {this.Address}:{this.Port}");
            OnClientConnected?.Invoke();
        }

        protected override void OnConnecting()
        {
            logger.LogInformation($"Rlogin Client is connecting to {this.Address}:{this.Port}");
            base.OnConnecting();
        }

        public void SendLogin(RloginConnectionInfo connectionInfo)
        {
            string AuthString;
            if (connectionInfo.IsDoorMode)
                AuthString = $"\0{connectionInfo.ClientUserName}\0{connectionInfo.ServerUserName}\0xtrn={connectionInfo.DoorName}\0";
            else
                AuthString = $"\0{connectionInfo.ClientUserName}\0{connectionInfo.ServerUserName}\0{connectionInfo.TerminalType}/{connectionInfo.TerminalBaudRate}\0";
            
            SendAsync(AuthString);
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            OnClientError?.Invoke(new Exception($"Rlogin Client caught an error code {error}.\n{Enum.GetName(typeof(System.Net.Sockets.SocketError), error)}"));
            base.OnError(error);
        }

        public override bool Disconnect()
        {
            base.Disconnect();
            OnClientDisconnected?.Invoke();
            rloginSessionEstablished = false;
            return !IsConnected;
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var workString = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

            if (!rloginSessionEstablished && workString.Trim() == "\0")
            {
                rloginSessionEstablished = true;
                return;
            }

            OnDataReceived?.Invoke(buffer, offset, size);
            base.OnReceived(buffer, offset, size);
        }
    }
}
