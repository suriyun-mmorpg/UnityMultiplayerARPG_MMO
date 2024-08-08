using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public static class ConfigManager
    {
        private static ServerConfig _serverConfig;
        private static ClientConfig _clientConfig;

        public static bool HasServerConfig()
        {
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            return File.Exists(configFilePath);
        }

        public static ServerConfig ReadServerConfig()
        {
            if (_serverConfig != null)
                return _serverConfig;
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            Debug.Log($"Reading server config file from {configFilePath}");
            if (File.Exists(configFilePath))
            {
                // Read config file
                Debug.Log("Found server config file.");
                string dataAsJson = File.ReadAllText(configFilePath);
                _serverConfig = JsonConvert.DeserializeObject<ServerConfig>(dataAsJson);
                return _serverConfig;
            }
            return new ServerConfig();
        }

        public static void WriteServerConfigIfNotExisted(ServerConfig writingConfig)
        {
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            Debug.Log("Not found server config file, creating a new one.");
            if (!Directory.Exists(configFolder))
                Directory.CreateDirectory(configFolder);
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(_serverConfig, Formatting.Indented));
        }

        public static ClientConfig ReadClientConfig()
        {
            return _clientConfig;
        }
    }
}