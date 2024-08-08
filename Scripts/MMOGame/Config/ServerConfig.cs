using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class ServerConfig
    {
        // Central server
        public string centralAddress;
        public int? centralPort;
        public int? centralMaxConnections;

        // Central channel
        public int? mapSpawnMillisecondsTimeout;
        public int? defaultChannelMaxConnections;
        public List<ChannelData> channels;

        // Central web-socket connection (for login/character management)
        public bool? useWebSocket;
        public bool? webSocketSecure;
        public string webSocketCertPath;
        public string webSocketCertPassword;

        // Cluster server
        public int? clusterPort;
        [System.Obsolete("Use `publicAddress` instead.")]
        public string machineAddress;
        public string publicAddress;

        // Map spawn server
        public int? mapSpawnPort;
        public string spawnExePath;
        public bool? notSpawnInBatchMode;
        public int? spawnStartPort;
        public List<string> spawnMaps;
        public List<string> spawnChannels;
        public List<SpawnAllocateMapByNameData> spawnAllocateMaps;

        // Map server
        public int? mapPort;
        public int? mapMaxConnections;

        // Database manager server
        public bool? useCustomDatabaseClient;
        public int? databaseOptionIndex;
        [System.Obsolete("Use `disableDatabaseCaching` instead.")]
        public bool? databaseDisableCacheReading;
        public bool? disableDatabaseCaching;

        public string databaseManagerAddress;
        public int? databaseManagerPort;
    }
}