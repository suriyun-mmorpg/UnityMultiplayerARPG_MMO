using System.Collections;
using System.Collections.Generic;
using Grpc.Core;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerServer
    {
        public Server Server { get; private set; }
        public BaseDatabase Database { get; private set; }

        public DatabaseManagerServer(int port, BaseDatabase database) : this(port, database, ServerCredentials.Insecure)
        {

        }

        public DatabaseManagerServer(int port, BaseDatabase database, ServerCredentials credentials)
        {
            Database = database;
            Server = new Server
            {
                Services = { DatabaseService.BindService(new DatabaseServiceImplement(database)) },
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
