using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public class MMOClientInstance : MonoBehaviour
    {
        public static MMOClientInstance Singleton { get; protected set; }

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
