using Cysharp.Threading.Tasks;
using Insthync.UnityRestClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace MultiplayerARPG.MMO
{
    public static class ConfigManager
    {
        public static string ClientConfigRemoteUrl { get; set; } = string.Empty;
        public static string ClientConfigRemoteDevUrl { get; set; } = string.Empty;
        private static bool s_IsLoadingClientConfig = false;
        private static readonly string CachedClientConfigFileName = "cachedClientConfig.json";
        private static string CachedClientConfigPath => Path.Combine(Application.persistentDataPath, CachedClientConfigFileName);
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
                File.WriteAllText(configFilePath, JsonConvert.SerializeObject(writingConfig, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
                Debug.Log($"Server config file written to {configFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unable to create a new config file, {ex}");
            }
        }

        public static async UniTask<bool> HasClientConfig()
        {
            return await HasTextFileInStreamingAssets("clientConfig.json");
        }

        public static async UniTask<ClientConfig> ReadClientConfig(bool reRead = false)
        {
            if (_clientConfig != null && !reRead)
                return _clientConfig;

            if (s_IsLoadingClientConfig)
            {
                do
                {
                    await UniTask.Delay(1000);
                } while (s_IsLoadingClientConfig);
                if (_clientConfig != null)
                    return _clientConfig;
            }

            // Get config file URLs
            string[] args = System.Environment.GetCommandLineArgs();
            string configRemoteUrl = string.Empty;
            if (ConfigReader.ReadArgs(args, ProcessArguments.ARG_CLIENT_CONFIG_URL, out configRemoteUrl, string.Empty))
            {
                ClientConfigRemoteUrl = ClientConfigRemoteDevUrl = configRemoteUrl;
            }
            else if (ConfigReader.ReadEnv(ProcessArguments.CONFIG_CLIENT_CONFIG_URL, out configRemoteUrl, string.Empty))
            {
                ClientConfigRemoteUrl = ClientConfigRemoteDevUrl = configRemoteUrl;
            }
            Debug.Log($"Reading remote client config from: \"{ClientConfigRemoteUrl}\", dev: \"{ClientConfigRemoteDevUrl}\"");

            string remoteConfigUrl = null;
            if (!Debug.isDebugBuild && !string.IsNullOrWhiteSpace(ClientConfigRemoteUrl))
                remoteConfigUrl = ClientConfigRemoteUrl;

            if (Debug.isDebugBuild && !string.IsNullOrWhiteSpace(ClientConfigRemoteDevUrl))
                remoteConfigUrl = ClientConfigRemoteDevUrl;

            // Read config file remotely
            if (!string.IsNullOrEmpty(remoteConfigUrl))
            {
                Debug.Log($"Read config file from {remoteConfigUrl}");
                if (!remoteConfigUrl.Contains("?"))
                    remoteConfigUrl += "?";
                else
                    remoteConfigUrl += "&";

                remoteConfigUrl += $"time={System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond}";
                remoteConfigUrl += $"&platform={Application.platform}";
                remoteConfigUrl += $"&version={Application.version}";
                remoteConfigUrl += $"&unity_version={Application.unityVersion}";

                s_IsLoadingClientConfig = true;
                RestClient.Result<ClientConfig> readConfigResult = await RestClient.Get<ClientConfig>(remoteConfigUrl);
                s_IsLoadingClientConfig = false;
                if (!readConfigResult.IsError())
                {
                    _clientConfig = readConfigResult.Content;

                    // Save config file to local path (must works for Android, iOS too)
                    try
                    {
                        Debug.Log($"Client config cached to: {CachedClientConfigPath}");
                        File.WriteAllText(CachedClientConfigPath, JsonConvert.SerializeObject(_clientConfig));
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to cache client config: {ex.Message}\n{ex.StackTrace}");
                    }

                    return _clientConfig;
                }
                else
                {
                    Debug.LogError($"Unable to read remote client config from: \"{remoteConfigUrl}\"");
                }
            }

            // Read saved config file
            if (!Application.isEditor)
            {
                if (File.Exists(CachedClientConfigPath))
                {
                    try
                    {
                        Debug.Log($"Read config file from persistent cache {CachedClientConfigPath}.");
                        string cachedJson = File.ReadAllText(CachedClientConfigPath);
                        return _clientConfig = JsonConvert.DeserializeObject<ClientConfig>(cachedJson);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"Failed to read cached config: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            // Read from streaming assets
            string configFileName = "editorClientConfig.json";
            if (!await HasTextFileInStreamingAssets(configFileName))
                configFileName = "clientConfig.json";

            try
            {
                Debug.Log($"Read config file from `StreamingAssets`");
                return _clientConfig = JsonConvert.DeserializeObject<ClientConfig>(await ReadTextFromStreamingAssets(configFileName));
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ConfigManager] Failed to read client config from `StreamingAssets` {ex.Message}\n{ex.StackTrace}");
            }

            return new ClientConfig();
        }

        public static async UniTask<List<MmoNetworkSetting>> ReadServerList()
        {
            List<MmoNetworkSetting> result = new List<MmoNetworkSetting>();
            string text = await ReadTextFromStreamingAssets("serverList.txt");
            if (string.IsNullOrWhiteSpace(text))
                return result;
            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);
            for (int i = 0; i < lines.Length; ++i)
            {
                // Split by any whitespace (space, tab, etc.)
                string[] parts = lines[i].Trim().Split(',');
                if (parts.Length < 2)
                    continue;
                bool webSocketSecure = false;
                if (parts.Length > 2)
                    bool.TryParse(parts[2], out webSocketSecure);
                string title = parts[0];
                string address = parts[1];
                string[] addressParts = address.Trim().Split(':');
                if (addressParts.Length < 2)
                    continue;
                string ip = addressParts[0];
                if (!int.TryParse(addressParts[1], out int port))
                    continue;
                MmoNetworkSetting setting = ScriptableObject.CreateInstance<MmoNetworkSetting>();
                setting.name = $"FromFile_{i}";
                setting.DefaultTitle = title;
                setting.networkAddress = ip;
                setting.networkPort = port;
                setting.webSocketSecure = webSocketSecure;
                result.Add(setting);
            }
            return result;
        }

        public static async UniTask<bool> HasTextFileInStreamingAssets(string fileName)
        {
            if (ShouldReadConfigByWebRequest())
            {
                // NOTE: Find better way to implement this one
                return await ReadTextFromStreamingAssets(fileName) != null;
            }
            else
            {
                string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
                return File.Exists(filePath);
            }
        }

        public static async UniTask<string> ReadTextFromStreamingAssets(string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
            Debug.Log($"[ConfigManager] Reading text from streaming assets {filePath}");
            if (ShouldReadConfigByWebRequest())
            {
                using (UnityWebRequest request = UnityWebRequest.Get(filePath))
                {
                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    do
                    {
                        await UniTask.Yield();
                    } while (!operation.isDone);
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string content = request.downloadHandler.text;
                        Debug.Log($"[ConfigManager] {filePath} Content:\n{content}");
                        return content;
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    Debug.Log($"[ConfigManager] {filePath} Content:\n{content}");
                    return content;
                }
            }
            return null;
        }

        public static bool ShouldReadConfigByWebRequest()
        {
            return !Application.isEditor && (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android);
        }
    }
}
