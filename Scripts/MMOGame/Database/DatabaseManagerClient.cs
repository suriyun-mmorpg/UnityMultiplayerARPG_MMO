using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Client = DatabaseService.DatabaseServiceClient;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerClient
    {
        public Channel Channel { get; private set; }
        public Client Client { get; private set; }

        public DatabaseManagerClient(string serverHost, int serverPort) : this(serverHost, serverPort, ChannelCredentials.Insecure)
        {

        }

        public DatabaseManagerClient(string serverHost, int serverPort, ChannelCredentials credentials)
        {
            Channel = new Channel(serverHost, serverPort, credentials);
            Client = new Client(Channel);
        }

        public async void ShutDown()
        {
            await Channel.ShutdownAsync();
        }
    }
}
