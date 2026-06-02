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
        public static string ClientConfigRemoteProdUrl { get; set; } = string.Empty;
        public static ClientConfigData ProdClientConfig { get; set; } = null;
        public static string ClientConfigRemoteDevUrl { get; set; } = string.Empty;
        public static ClientConfigData DevClientConfig { get; set; } = null;
        public static bool ForceUseDevClientConfig { get; set; } = false;
        public static string ClientConfigRemoteStagingUrl { get; set; } = string.Empty;
        public static ClientConfigData StagingClientConfig { get; set; } = null;
        public static bool ForceUseStagingClientConfig { get; set; } = false;

        private static ServerConfig _serverConfig = null;
        private static ClientConfig _clientConfig = null;

        private static bool s_IsLoadingClientConfig = false;
        private static readonly string CachedClientConfigFileName = "cachedClientConfig.json";
        private static readonly string StreamingEditorClientConfigFileName = "editorClientConfig.json";
        private static readonly string StreamingClientConfigFileName = "clientConfig.json";
        private static string CachedClientConfigPath => Path.Combine(Application.persistentDataPath, CachedClientConfigFileName);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            ClientConfigRemoteProdUrl = string.Empty;
            ProdClientConfig = null;
            ClientConfigRemoteDevUrl = string.Empty;
            DevClientConfig = null;
            ForceUseDevClientConfig = false;
            ClientConfigRemoteStagingUrl = string.Empty;
            StagingClientConfig = null;
            ForceUseStagingClientConfig = false;

            _serverConfig = null;
            _clientConfig = null;

            s_IsLoadingClientConfig = false;
        }

        public static bool HasServerConfig()
        {
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            return File.Exists(configFilePath);
        }

        public static ServerConfig ReadServerConfig(bool reRead = false)
        {
            if (_serverConfig != null && !reRead)
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
            if (!Directory.Exists(configFolder))
            {
                Debug.Log($"Not found server config file, creating a new one.\n{writingConfig}");
                Directory.CreateDirectory(configFolder);
            }
            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(writingConfig, Formatting.Indented));
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
                ClientConfigRemoteProdUrl = ClientConfigRemoteDevUrl = ClientConfigRemoteStagingUrl = configRemoteUrl;
            }
            else if (ConfigReader.ReadEnv(ProcessArguments.CONFIG_CLIENT_CONFIG_URL, out configRemoteUrl, string.Empty))
            {
                ClientConfigRemoteProdUrl = ClientConfigRemoteDevUrl = ClientConfigRemoteStagingUrl = configRemoteUrl;
            }
            Debug.Log($"Reading remote client config from: \"{ClientConfigRemoteProdUrl}\", develop: \"{ClientConfigRemoteDevUrl}\", staging: \"{ClientConfigRemoteStagingUrl}\", version: \"{Application.version}\"");

            bool isDevelopVersion = Application.version.ToLower().Contains("develop");
            bool isStagingVersion = Application.version.ToLower().Contains("staging");
            string remoteConfigUrl = null;

            if ((ForceUseDevClientConfig || isDevelopVersion) && !string.IsNullOrWhiteSpace(ClientConfigRemoteDevUrl))
                remoteConfigUrl = ClientConfigRemoteDevUrl;
            else if ((ForceUseStagingClientConfig || isStagingVersion) && !string.IsNullOrWhiteSpace(ClientConfigRemoteStagingUrl))
                remoteConfigUrl = ClientConfigRemoteStagingUrl;
            else if (!isDevelopVersion && !isStagingVersion && !string.IsNullOrWhiteSpace(ClientConfigRemoteProdUrl))
                remoteConfigUrl = ClientConfigRemoteProdUrl;

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

                    if (!Application.isEditor)
                        return _clientConfig;
                }
                else
                {
                    Debug.LogError($"Unable to read remote client config from: \"{remoteConfigUrl}\"");
                }
            }

            // Read from streaming assets
            string configFileName = StreamingEditorClientConfigFileName;
            if (!await HasTextFileInStreamingAssets(configFileName))
                configFileName = StreamingClientConfigFileName;

            if (await HasTextFileInStreamingAssets(configFileName))
            {
                try
                {
                    Debug.Log($"Read config file from `StreamingAssets`");
                    return _clientConfig = JsonConvert.DeserializeObject<ClientConfig>(await ReadTextFromStreamingAssets(configFileName));
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[ConfigManager] Failed to read client config from `StreamingAssets` {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogWarning($"[ConfigManager] Unable to read {configFileName}, so it will use default config");
            }

            // Read saved config file
            if (!isDevelopVersion && !isStagingVersion && !Application.isEditor)
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

            if ((ForceUseDevClientConfig || isDevelopVersion) && DevClientConfig != null)
            {
                _clientConfig = DevClientConfig.config;
                return _clientConfig;
            }
            else if ((ForceUseStagingClientConfig || isStagingVersion) && StagingClientConfig != null)
            {
                _clientConfig = StagingClientConfig.config;
                return _clientConfig;
            }
            else if (ProdClientConfig != null)
            {
                _clientConfig = ProdClientConfig.config;
                return _clientConfig;
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
            string configFilePath = Path.Combine(Application.streamingAssetsPath, fileName);
            Debug.Log($"[ConfigManager] Reading text from streaming assets {configFilePath}");
            if (ShouldReadConfigByWebRequest())
            {
                using (UnityWebRequest request = UnityWebRequest.Get(configFilePath))
                {
                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    do
                    {
                        await UniTask.Delay(1000);
                    } while (!operation.isDone);
                    if (request.result == UnityWebRequest.Result.Success)
                        return request.downloadHandler.text;
                }
            }
            else
            {
                if (File.Exists(configFilePath))
                    return File.ReadAllText(configFilePath);
            }
            return null;
        }

        public static bool ShouldReadConfigByWebRequest()
        {
            return !Application.isEditor && (Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android);
        }
    }
}
