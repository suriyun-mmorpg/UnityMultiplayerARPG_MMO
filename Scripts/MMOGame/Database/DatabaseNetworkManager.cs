using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class DatabaseNetworkManager : MonoBehaviour
    {
        [SerializeField]
        private BaseDatabase database;
        [SerializeField]
        private BaseDatabase[] databaseOptions;
        [SerializeField]
        private string networkAddress = "localhost";
        [SerializeField]
        private int networkPort = 7770;
        
        public DatabaseManagerClient Client { get; private set; }
        public DatabaseManagerServer Server { get; private set; }
        public BaseDatabase Database { get { return database == null ? databaseOptions.FirstOrDefault() : database; } }

        public void SetDatabaseByOptionIndex(int index)
        {
            if (databaseOptions != null &&
                databaseOptions.Length > 0 &&
                index >= 0 &&
                index < databaseOptions.Length)
                database = databaseOptions[index];
        }

        public void StartServer()
        {
            if (Server != null)
                Server.ShutDown();
            Server = new DatabaseManagerServer(networkPort, Database);
            Server.Start();
        }

        public void StartClient()
        {
            if (Client != null)
                Client.ShutDown();
            Client = new DatabaseManagerClient(networkAddress, networkPort);
        }

        public DatabaseService.DatabaseServiceClient ServiceClient
        {
            get { return Client.Client; }
        }
    }
}
