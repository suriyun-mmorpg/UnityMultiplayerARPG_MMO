using System.Collections;
using System.Collections.Generic;
using Grpc.Core;

namespace MultiplayerARPG.MMO
{
    public class DatabaseServer
    {
        private Server server;

        public DatabaseServer(int port) : this(port, ServerCredentials.Insecure)
        {

        }

        public DatabaseServer(int port, ServerCredentials credentials)
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
