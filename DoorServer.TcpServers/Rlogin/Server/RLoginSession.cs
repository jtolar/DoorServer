using DoorServer.TcpServers.Rlogin.Client;
using DoorServer.TcpServers.SshTunnel;
using System.Text;

namespace DoorServer.TcpServers.Rlogin.Server
{
    public class RLoginSession : TcpSession
    {
        private RloginServer server;
        private SshTunnelClient? sshTunnel;
        private RloginClient? rloginClient;
        private ILogger logger;
        public ISshTunnelConfiguration tunnelConfiguration;
        private bool echoServiceMessageSent = false;
        private bool IsTunnelEstablished = false;

        public RLoginSession(ILogger logger, RloginServer server,
            ISshTunnelConfiguration tunnelConfiguration) : base(server)
        {
            this.server = server;
            this.logger = logger;
            this.tunnelConfiguration = tunnelConfiguration;
            ConnectionInfo = new RloginConnectionInfo();
        }

        private RloginConnectionInfo ConnectionInfo { get; set; }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            var rawString = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            if (Debugger.IsAttached)
                Debug.WriteLine($"Recieved String: {rawString.Replace("\0", "\\null")}");

            ConsoleWrite.RloginServerMessage($"[RloginServer Received] {rawString}");

            if (!ConnectionInfo.IsConnected && rawString.StartsWith(char.MinValue))
            {
                var loginString = rawString.Split(char.MinValue, StringSplitOptions.None);

                if (ParseRloginConnectionInfo(rawString))
                {
                    //Send null byte to acknowledge connection.
                    ConsoleWrite.RloginServerMessage("Sending ACK");
                    Send("\0");
                    ConnectionInfo.IsConnected = true;
                    if (ConnectionInfo.IsDoorMode)
                    {
                        ConsoleWrite.RloginServerMessage($"Sending Connection accepted for user {ConnectionInfo.ServerUserName} in door mode ({ConnectionInfo.DoorName}) \r\n");
                        Send($"Connection accepted for user {ConnectionInfo.ServerUserName} in door mode ({ConnectionInfo.DoorName}) \r\n");
                    }
                    else
                    {
                        ConsoleWrite.RloginServerMessage($"Sending Connection accepted for user {ConnectionInfo.ServerUserName} in door mode ({ConnectionInfo.DoorName}) \r\n");
                        Send($"Connection accepted for user {ConnectionInfo.ServerUserName} in client mode \r\n");
                    }
                    var ansBytes = File.ReadAllBytes($"{Environment.CurrentDirectory}\\logon_success.ans");
                    ConsoleWrite.RloginServerMessage("Sending Ansi file");
                    Send(ansBytes, 0, ansBytes.LongLength);

                }
                else
                {
                    ConsoleWrite.RloginServerMessage($"Error parsing login information. Disconnecting session {Id}!");
                    //Console.WriteLine("Error parsing login information. Disconnecting!");
                    Send("Error parsing login information. Disconnecting!\r\n");
                    Disconnect();
                    return;
                }
            }

            if (rawString.StartsWith("~"))
            {
                ConsoleWrite.RloginServerMessage($"Escape character ~ recieved. Disconnecting session {Id}!\r\n");
                ConsoleWrite.RloginServerMessage("Sending Logoff message");
                Send($"Escape character ~ recieved. Disconnecting!\r\n");
                Send($"Goodbye\r\n");
                Disconnect();
                return;
            }

            if (IsTunnelEstablished && ConnectionInfo.IsDoorMode && rloginClient.IsConnected)
            {
                ConsoleWrite.RloginClientMessage($"[RloginServer Sent] {rawString.Replace("\0", "\\null")}");
                ConsoleWrite.RloginServerMessage($"Sending - {rawString}");
                rloginClient.SendAsync(rawString);
                //var result = tunnelClient.CreateCommand(rawString);
                //Send(result.Result);
                return;
            }

            if (ConnectionInfo.IsDoorMode && !IsTunnelEstablished)
            {
                CreateSshTunnel();
                sshTunnel.StartTunnel();
                return;
            }

            //if (ConnectionInfo.IsDoorMode)
            //{
            //    Console.WriteLine($"Creating Rlogin Proxy for session {Id}");
            //    //var tunnel = new DoorPartyProxy(logger);
            //    //tunnel.OnTunnelError += Tunnel_OnTunnelError;
            //    //tunnel.StartTunnel(this);
            //    //Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            //    //tunnel.StopTunnel();
            //    return;
            //}

            if (!echoServiceMessageSent)
            {
                ConsoleWrite.RloginServerMessage("Sending Echo Server Message");
                Send("Echo Service (use the ~ character to end session\r\n");
                echoServiceMessageSent = true;
            }

            if (rawString.StartsWith("~"))
            {
                ConsoleWrite.RloginServerMessage($"Escape character ~ recieved. Disconnecting session {Id}!\r\n");
                Send($"Escape character ~ recieved. Disconnecting!\r\n");
                Send($"Goodbye\r\n");
                Disconnect();
                return;
            }
            ConsoleWrite.RloginServerMessage($"Sending Echo Reply: {rawString}");
            Send($"[ECHO] {rawString}\r\n");

            //Console.WriteLine($"Disconnecting session {Id}!\r\n");
            //Send("Disconnecting!\r\n");
            //Send($"Goodbye\r\n");
            //Disconnect();
        }

        private bool ParseRloginConnectionInfo(string rawString)
        {
            if (Debugger.IsAttached)
                Debug.WriteLine("Parsing Connection Info ...");
            //https://datatracker.ietf.org/doc/html/rfc1282
            //According to protocol we should recieve a login string consisting of a null byte
            //followed by 3 null byte terminated parameters.
            //Example :
            //<null>
            //client-user-name <null>
            //server-user-name <null>
            //terminal-type/speed <null>
            try
            {
                //To Test door mode config from terminal program uncomment this.
                if (!rawString.Contains("xtrn"))
                {
                    ConnectionInfo.DoorName = "lord2";
                    ConnectionInfo.IsDoorMode = true;
                }


                var loginString = rawString.Split(char.MinValue, StringSplitOptions.None);

                if (rawString.Contains("xtrn=") || ConnectionInfo.IsDoorMode)
                {
                    ConnectionInfo.ClientUserName = string.IsNullOrWhiteSpace(loginString[1]) 
                        ? loginString[2].Replace(tunnelConfiguration.SystemTag, string.Empty)
                        : loginString[1];

                    ConnectionInfo.ServerUserName = loginString[2].Contains(tunnelConfiguration.SystemTag) 
                        ? loginString[2]
                        : $"[{tunnelConfiguration.SystemTag.Replace("[", string.Empty).Replace("]", string.Empty)}]{loginString[2]}";
                    
                    //if we uncommented the test mode code above then we don't want to overwrite the door name
                    if (string.IsNullOrWhiteSpace(ConnectionInfo.DoorName))
                        ConnectionInfo.DoorName = loginString[3].ToLower().Replace("xtrn=", string.Empty);

                    if (loginString.Length > 4)
                    {
                        ConnectionInfo.AdditionalOptions = loginString[4];
                    }
                    ConnectionInfo.IsDoorMode = true;
                }
                else
                {
                    ConnectionInfo.ClientUserName = loginString[1];
                    ConnectionInfo.ServerUserName = loginString[2];
                    if (!string.IsNullOrWhiteSpace(loginString[3]))
                    {
                        if (loginString[3].Contains("/"))
                        {
                            ConnectionInfo.TerminalType = loginString[3].Substring(0, loginString[3].IndexOf("/"));
                            ConnectionInfo.TerminalBaudRate = Convert.ToInt32(loginString[3].Substring(loginString[3].IndexOf("/") + 1));
                        }
                        else
                        {
                            ConnectionInfo.TerminalType = loginString[3];
                            ConnectionInfo.TerminalBaudRate = 14400;
                        }

                        if (loginString.Length > 4)
                        {
                            ConnectionInfo.AdditionalOptions = loginString[4];
                        }

                        ConnectionInfo.IsDoorMode = false;
                    }
                }
                                
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error Parsing Rlogin Connection Info: {ex.GetBaseException().Message}");
                return false;
            }
        }

        protected override void OnConnected()
        {
            logger.LogInformation($"Rlogin session {Id} connected... checking connections count. Currently {server.SessionDictionary.Count} of allowed {server.serverConfiguration.MaxConnections} are in use.");
            if (server.SessionDictionary.Count >= server.serverConfiguration.MaxConnections)
            {
                Send("\0"); //Send ACK so we can send full message and disconnect.
                logger.LogInformation($"Rlogin session {Id} exceeds the number of allowed connections (allowed: {server.serverConfiguration.MaxConnections}).");
                Send($"The server is full at the moment. Please try again later. (Maximum allowed: {server.serverConfiguration.MaxConnections})");
                Disconnect();

                return;
            }

            //server.SessionDictionary.Add(Id, this);
            ConsoleWrite.RloginServerMessage($"Rlogin session {Id} connected!");
        }

        protected override void OnConnecting()
        {
            if (Socket.RemoteEndPoint != null)
            {
                var remoteEndpoint = (IPEndPoint)Socket.RemoteEndPoint;
                ConnectionInfo.RemotePort = (uint)remoteEndpoint.Port;
                ConnectionInfo.RemoteIpAddressString = remoteEndpoint.Address.MapToIPv4().ToString();
                logger.LogInformation($"Rlogin session {Id} connecting from {ConnectionInfo.RemoteIpAddressString} on port {remoteEndpoint.Port}!");
                if (Socket.LocalEndPoint != null)
                {
                    var localEndpoint = (IPEndPoint)Socket.LocalEndPoint;
                    ConnectionInfo.LocalPort = (uint)localEndpoint.Port;
                    ConnectionInfo.LocalIpAddressString = localEndpoint.Address.MapToIPv4().ToString();
                }
            }
            else
            {
                logger.LogInformation("Unable to determine remote endpoint info... ABORTING SESSION!");
                Send("Unable to determine remote endpoint info... ABORTING!");
                Disconnect();
            }
        }

        protected override void OnDisconnected()
        {
            if (server.SessionDictionary.ContainsKey(Id))
                server.SessionDictionary.Remove(Id);

            ConnectionInfo.IsConnected = false;
            logger.LogInformation($"Rlogin Session {Id} Disconnected!");
        }

        protected override void OnDisconnecting()
        {
            if (sshTunnel != null &&
                (sshTunnel.IsSshServerConnected ||
                sshTunnel.IsTunnelConnected))
                sshTunnel.StopTunnel();

            logger.LogInformation($"Rlogin Session {Id} Disconnecting!");
        }

        public override bool Disconnect()
        {
            if (sshTunnel != null &&
                (sshTunnel.IsSshServerConnected ||
                sshTunnel.IsTunnelConnected))
                sshTunnel.StopTunnel();

            return base.Disconnect();
        }

        protected override void OnError(System.Net.Sockets.SocketError error)
        {
            logger.LogError($"[ERROR] Rlogin Session caught an error code {error}.\n{Enum.GetName(typeof(System.Net.Sockets.SocketError), error)}");
            //Console.WriteLine($"Rlogin Session caught an error code {error}.\n{Enum.GetName(typeof(System.Net.Sockets.SocketError), error)}");
            //base.OnError(error);
        }

        private void CreateSshTunnel()
        {
            sshTunnel = new SshTunnelClient(ConnectionInfo, tunnelConfiguration);
            CreateRloginClient();

            sshTunnel.OnClientConnected += (client) => ConsoleWrite.SshClientMessage("Ssh client connection has been established with the server.");
            sshTunnel.OnClientConnecting += () => ConsoleWrite.SshClientMessage("Ssh client is establishing a connection to server.");
            sshTunnel.OnClientDisconnecting += (client) => ConsoleWrite.SshClientMessage("Disconnecting Ssh client connection to the server.");
            sshTunnel.OnClientDisconnected += () => ConsoleWrite.SshClientMessage("Ssh client connection to the server has been disconnected.");
            sshTunnel.OnTunnelConnecting += (client) => Console.WriteLine("Establishing tunnel connection to server.");
            sshTunnel.OnTunnelConnected += (client) =>
            {
                IsTunnelEstablished = true;
                logger.LogInformation("Tunnel connection to the server has been established.");
                if (rloginClient != null)
                    rloginClient.ConnectAsync();
            };
            sshTunnel.OnTunnelCreation += () => logger.LogInformation("Creating tunnel for connection.");
            sshTunnel.OnTunnelCreated += () => logger.LogInformation("Tunnel for connection has been created.");
            sshTunnel.OnTunnelDisconnecting += () => logger.LogInformation("Disconnecting tunnel connection to the server.");
            sshTunnel.OnTunnelDisconnected += () =>
            {
                IsTunnelEstablished = false;
               // tunnelClient = null;
                logger.LogInformation("Tunnel connection to the server has been disconnected.");
            };
            sshTunnel.OnTunnelError += (ex) =>
            {
                logger.LogInformation($"[TUN ERROR] {ex.GetBaseException().Message}");
                sshTunnel.StopTunnel();
            };
            sshTunnel.OnClientConnectionError += (ex) =>
            {
                ConsoleWrite.SshClientMessage($"[CONNECTION ERROR] {ex.GetBaseException().Message}");
                sshTunnel.StopTunnel();
            };
            sshTunnel.OnForwardRequestReceived += (request) =>
            {
                logger.LogInformation($"Forward Request Received: Origninator {request.OriginatorHost}:{request.OriginatorPort}");

                if (rloginClient != null && rloginClient.IsConnected)
                    rloginClient.SendLogin(ConnectionInfo);
            };

            sshTunnel.CreateTunnel();
        }

        private void CreateRloginClient()
        {
            //var host = Dns.GetHostAddresses("dp.throwbackbbs.com")[0].MapToIPv4();
            rloginClient = new RloginClient(logger, IPAddress.Loopback, (int)ConnectionInfo.RemotePort);
            rloginClient.OnDataReceived += (buffer, offset, size) =>
            {
                Send(buffer, offset, size);
                ConsoleWrite.RloginClientMessage(Encoding.UTF8.GetString(buffer, (int)offset, (int)size).Replace("\0", "\\null"));
            };
            rloginClient.OnClientConnected += () => ConsoleWrite.RloginClientMessage("Rlogin client connection has been established with the server.");
            rloginClient.OnClientDisconnected += () =>
            {
                this.Disconnect();
                ConsoleWrite.RloginClientMessage("Rlogin client connection to the server has been disconnected.");
            };
            rloginClient.OnClientError += (ex) => ConsoleWrite.RloginClientMessage(ex.Message);
        }
    }
}