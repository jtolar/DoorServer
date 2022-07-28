namespace DoorServer.TcpServers.Rlogin.Server
{
    public class RloginConnectionInfo
    {
        public RloginConnectionInfo()
        {
            DoorName = string.Empty;
            TerminalType = "ANSI";
            IsDoorMode = false;
            IsConnected = false;
            ClientUserName = string.Empty;
            ServerUserName = string.Empty;
            TerminalBaudRate = 115200;
            AdditionalOptions = string.Empty;
        }

        public string ClientUserName { get; set; }

        public string ServerUserName { get; set; }

        public string TerminalType { get; set; }

        public int TerminalBaudRate { get; set; }

        public string RemoteIpAddressString { get; set; }

        public IPAddress? RemoteIpAddress
        {
            get
            {
                return string.IsNullOrWhiteSpace(RemoteIpAddressString)
                    ? IPAddress.Any.MapToIPv4()
                    : IPAddress.Parse(RemoteIpAddressString).MapToIPv4();

            }
        }

        public uint RemotePort { get; set; } = 0;

        public string LocalIpAddressString { get; set; }

        public IPAddress? LocalIpAddress
        {
            get
            {
                return string.IsNullOrWhiteSpace(LocalIpAddressString)
                    ? IPAddress.Any.MapToIPv4()
                    : IPAddress.Parse(LocalIpAddressString).MapToIPv4();

            }
        }

        public uint LocalPort { get; set; } = 0;

        public string AdditionalOptions { get; set; }

        public bool IsConnected { get; set; }

        public bool IsDoorMode { get; set; }

        public string DoorName { get; set; }

    }
}