using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public class MMOGameInstance : MonoBehaviour
    {
        public static MMOGameInstance Singleton { get; protected set; }

        #region Server components
        [Header("Server Components")]
        public CentralNetworkManager centralServerNetworkManager;
        public LoginNetworkManager loginServerNetworkManager;
        public ChatNetworkManager chatServerNetworkManager;
        public MapSpawnNetworkManager mapSpawnServerNetworkManager;
        public MapNetworkManager mapServerNetworkManager;
        #endregion

        #region Client components
        [Header("Client Components")]
        public CentralNetworkManager centralClientNetworkManager;
        public LoginNetworkManager loginClientNetworkManager;
        public ChatNetworkManager chatClientNetworkManager;
        public MapSpawnNetworkManager mapSpawnClientNetworkManager;
        public MapNetworkManager mapClientNetworkManager;
        #endregion

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
        }

        #region Server functions
        public void StartCentralServer()
        {
            centralServerNetworkManager.StartServer();
        }

        public void StartLoginServer()
        {
            loginServerNetworkManager.StartServer();
        }

        public void StartChatServer()
        {
            chatServerNetworkManager.StartServer();
        }

        public void StartMapSpawnServer()
        {
            mapSpawnServerNetworkManager.StartServer();
        }

        public void StartMapServer()
        {
            mapServerNetworkManager.StartServer();
        }
        #endregion

        #region Client functions
        public void StartCentralClient()
        {
            centralClientNetworkManager.StartClient();
        }

        public void StartLoginClient()
        {
            loginClientNetworkManager.StartClient();
        }

        public void StartChatClient()
        {
            chatClientNetworkManager.StartClient();
        }

        public void StartMapSpawnClient()
        {
            mapSpawnClientNetworkManager.StartClient();
        }

        public void StartMapClient()
        {
            mapClientNetworkManager.StartClient();
        }
        #endregion
    }
}
