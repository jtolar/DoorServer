namespace DoorServer.TcpServers
{
    public interface ILoginServerConfiguration : ITcpServerConfiguration
    {
        public int MaxConnections { get; set; }
    }
}
