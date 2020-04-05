using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Client = DatabaseService.DatabaseServiceClient;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerClient
    {
        private Channel channel;
        private Client client;

        public DatabaseManagerClient(string serverHost, int serverPort) : this(serverHost, serverPort, ChannelCredentials.Insecure)
        {

        }

        public DatabaseManagerClient(string serverHost, int serverPort, ChannelCredentials credentials)
        {
            channel = new Channel(serverHost, serverPort, credentials);
            client = new Client(channel);
        }

        public async void ShutDown()
        {
            await channel.ShutdownAsync();
        }
    }
}
