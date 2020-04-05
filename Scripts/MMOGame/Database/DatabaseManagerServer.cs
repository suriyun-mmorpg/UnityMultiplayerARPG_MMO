using System.Collections;
using System.Collections.Generic;
using Grpc.Core;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerServer
    {
        private Server server;

        public DatabaseManagerServer(int port) : this(port, ServerCredentials.Insecure)
        {

        }

        public DatabaseManagerServer(int port, ServerCredentials credentials)
        {
            server = new Server
            {
                Services = { DatabaseService.BindService(new DatabaseServiceImplement()) },
                Ports = { new ServerPort("localhost", port, credentials) }
            };
        }

        public async void ShutDown()
        {
            await server.ShutdownAsync();
        }
    }
}
