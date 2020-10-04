#if UNITY_STANDALONE && !CLIENT_BUILD
using Grpc.Core;
using Client = DatabaseService.DatabaseServiceClient;

namespace MultiplayerARPG.MMO
{
    public class DatabaseManagerClient
    {
        public Channel Channel { get; private set; }
        public Client Client { get; private set; }
        public ChannelState ChannelState { get { return Channel.State; } }

        public DatabaseManagerClient(string serverHost, int serverPort) : this(serverHost, serverPort, ChannelCredentials.Insecure)
        {

        }

        public DatabaseManagerClient(string serverHost, int serverPort, ChannelCredentials credentials)
        {
            Channel = new Channel(serverHost, serverPort, credentials);
            Client = new Client(Channel);
        }

        public async void Connect()
        {
            await Channel.ConnectAsync();
        }

        public async void ShutDown()
        {
            await Channel.ShutdownAsync();
        }
    }
}
#endif