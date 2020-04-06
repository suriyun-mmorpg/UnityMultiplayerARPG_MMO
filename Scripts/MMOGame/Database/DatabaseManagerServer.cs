using System.Collections;
using System.Collections.Generic;
using Grpc.Core;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerServer
    {
        public Server Server { get; private set; }

        public DatabaseManagerServer(int port) : this(port, ServerCredentials.Insecure)
        {

        }

        public DatabaseManagerServer(int port, ServerCredentials credentials)
        {
            Server = new Server
            {
                Services = { DatabaseService.BindService(new DatabaseServiceImplement()) },
                Ports = { new ServerPort("localhost", port, credentials) }
            };
        }

        public void Start()
        {
            Server.Start();
        }

        public async void ShutDown()
        {
            await Server.ShutdownAsync();
        }
    }
}
