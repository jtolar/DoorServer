namespace DoorServer.TcpServers
{
    public class LoginServerConfiguration : TcpServerConfiguration, ILoginServerConfiguration
    {
        public int MaxConnections { get ; set; }
    }
}
