using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
            try
            {
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(writingConfig, Formatting.Indented));
                Debug.Log($"Server config file written to {configFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unable to create a new config file, {ex}");
            }
        }

        public static bool HasClientConfig()
        {
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "clientConfig.json");
            return File.Exists(configFilePath);
        }

        public static ClientConfig ReadClientConfig()
        {
            string configFilePath = Path.Combine(Application.streamingAssetsPath, "clientConfig.json");
            Debug.Log($"Reading client config file from {configFilePath}");
            if (File.Exists(configFilePath))
            {
                // Read config file
                Debug.Log("Found client config file.");
                string dataAsJson = File.ReadAllText(configFilePath);
                _clientConfig = JsonConvert.DeserializeObject<ClientConfig>(dataAsJson);
                return _clientConfig;
            }
            return new ClientConfig();
        }
    }
}